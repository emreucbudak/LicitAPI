using System.Text;
using System.Text.Json;
using FlashMediator;
using Licit.MailService.API.Integrations;
using Licit.MailService.Application.DTOs;
using Licit.MailService.Application.Features.CQRS.Email.Commands.Send;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Licit.MailService.API.BackgroundServices;

public class BiddingOutbidEmailEventConsumerService(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<BiddingOutbidEmailEventConsumerService> logger) : BackgroundService
{
    private const string ExchangeName = "licit.events";
    private const string QueueName = "mail-service.bidding-outbid-email";
    private const string RoutingKey = "bidding.bid.outbid-email.requested";

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
            var connection = await factory.CreateConnectionAsync(stoppingToken);
            var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
            await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
            await channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var eventData = JsonSerializer.Deserialize<BiddingOutbidEmailEvent>(
                        body,
                        new JsonSerializerOptions(JsonSerializerDefaults.Web));

                    if (eventData is not null)
                    {
                        await ProcessEventAsync(eventData, stoppingToken);
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Bidding outbid email event could not be processed.");
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                }
            };

            await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, stoppingToken);
            logger.LogInformation("Bidding outbid email consumer started. Queue: {QueueName}", QueueName);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Bidding outbid email consumer RabbitMQ connection failed.");
        }
    }

    private async Task ProcessEventAsync(
        BiddingOutbidEmailEvent eventData,
        CancellationToken cancellationToken)
    {
        if (eventData.RecipientUserIds.Count == 0)
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var emailLookupClient = scope.ServiceProvider.GetRequiredService<AuthUserEmailLookupClient>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var recipients = await emailLookupClient.GetEmailsAsync(
            eventData.RecipientUserIds,
            cancellationToken);

        foreach (var recipient in recipients.Where(recipient => !string.IsNullOrWhiteSpace(recipient.Email)))
        {
            await mediator.Send(new SendEmailCommandRequest(
                recipient.Email,
                BiddingOutbidEmailTemplate.BuildSubject(),
                BiddingOutbidEmailTemplate.BuildBody(eventData)
            ));
        }

        logger.LogInformation(
            "Bidding outbid emails processed. AuctionId: {AuctionId}, RecipientCount: {RecipientCount}",
            eventData.AuctionId,
            recipients.Count);
    }
}
