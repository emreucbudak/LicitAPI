using System.Text;
using System.Text.Json;
using FlashMediator;
using Licit.MailService.Application.DTOs;
using Licit.MailService.Application.Features.CQRS.Email.Send;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Licit.MailService.API.BackgroundServices;

public class AuthLoginTwoFactorEventConsumerService(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<AuthLoginTwoFactorEventConsumerService> logger) : BackgroundService
{
    private const string ExchangeName = "licit.events";
    private const string QueueName = "mail-service.auth-login-2fa-events";
    private static readonly string[] RoutingKeys =
    [
        "auth.login.2fa.#"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMq:Port"] ?? "5672"),
            UserName = configuration["RabbitMq:Username"] ?? "licit",
            Password = configuration["RabbitMq:Password"] ?? "LicitDev2024!"
        };

        try
        {
            using var connection = await factory.CreateConnectionAsync(stoppingToken);
            using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

            foreach (var routingKey in RoutingKeys)
            {
                await channel.QueueBindAsync(QueueName, ExchangeName, routingKey, cancellationToken: stoppingToken);
            }

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var eventData = JsonSerializer.Deserialize<AuthLoginTwoFactorEmailEvent>(body, JsonOptions);

                    if (eventData is null)
                    {
                        logger.LogWarning("Auth login 2FA event could not be deserialized.");
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(eventData.Email) || string.IsNullOrWhiteSpace(eventData.Code))
                    {
                        logger.LogWarning("Auth login 2FA event is missing required fields.");
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                        return;
                    }

                    using var scope = scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    await mediator.Send(new SendEmailCommandRequest(
                        eventData.Email,
                        AuthLoginTwoFactorEmailTemplate.BuildSubject(),
                        AuthLoginTwoFactorEmailTemplate.BuildBody(eventData)
                    ));

                    logger.LogInformation("Auth login 2FA email processed for {Email}", eventData.Email);
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (JsonException ex)
                {
                    logger.LogWarning(ex, "Invalid auth login 2FA event payload received.");
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Auth login 2FA event processing failed.");
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, stoppingToken);
            logger.LogInformation("RabbitMQ auth login 2FA consumer started: {Queue}", QueueName);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RabbitMQ auth login 2FA connection error.");
        }
    }
}
