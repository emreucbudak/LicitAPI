using System.Text;
using System.Text.Json;
using FlashMediator;
using Licit.MailService.Application.DTOs;
using Licit.MailService.Application.Features.CQRS.Email.Commands.Send;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Licit.MailService.API.BackgroundServices;

public class AuthPasswordResetEventConsumerService(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<AuthPasswordResetEventConsumerService> logger) : BackgroundService
{
    private const string ExchangeName = "licit.events";
    private const string QueueName = "mail-service.auth-password-reset-events";
    private static readonly string[] RoutingKeys =
    [
        "auth.password-reset.#"
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
                    var eventData = JsonSerializer.Deserialize<AuthPasswordResetEmailEvent>(body, JsonOptions);

                    if (eventData is null || string.IsNullOrWhiteSpace(eventData.Email) || string.IsNullOrWhiteSpace(eventData.Code))
                    {
                        logger.LogWarning("Auth parola sıfırlama olayı geçersiz.");
                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                        return;
                    }

                    using var scope = scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    await mediator.Send(new SendEmailCommandRequest(
                        eventData.Email,
                        AuthPasswordResetEmailTemplate.BuildSubject(),
                        AuthPasswordResetEmailTemplate.BuildBody(eventData)
                    ));

                    logger.LogInformation("Auth parola sıfırlama e-postası işlendi. E-posta: {Email}", eventData.Email);
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (JsonException ex)
                {
                    logger.LogWarning(ex, "Geçersiz auth parola sıfırlama olayı yükü alındı.");
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Auth parola sıfırlama olayı işlenemedi.");
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, stoppingToken);
            logger.LogInformation("RabbitMQ auth parola sıfırlama tüketicisi başlatıldı: {Queue}", QueueName);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RabbitMQ auth parola sıfırlama bağlantı hatası.");
        }
    }
}
