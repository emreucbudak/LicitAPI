using System.Text;
using System.Text.Json;
using FlashMediator;
using Licit.MailService.Application.Features.CQRS.Email.Send;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Licit.MailService.API.BackgroundServices;

public class TenderEventConsumerService(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<TenderEventConsumerService> logger) : BackgroundService
{
    private const string ExchangeName = "licit.events";
    private const string QueueName = "mail-service.tender-events";

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
            await channel.QueueBindAsync(QueueName, ExchangeName, "tender.status.*", cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var eventData = JsonSerializer.Deserialize<TenderStatusChangedEvent>(body);

                    if (eventData is not null)
                    {
                        logger.LogInformation("Alındı: TenderStatusChanged - {TenderId} -> {Status}", eventData.TenderId, eventData.NewStatus);

                        using var scope = scopeFactory.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                        await mediator.Send(new SendEmailCommandRequest(
                            "admin@licit.com",
                            $"İhale Durum Değişikliği: {eventData.Title}",
                            $"İhale '{eventData.Title}' durumu '{eventData.NewStatus}' olarak değiştirildi. Tarih: {eventData.OccurredAt:dd.MM.yyyy HH:mm}"
                        ));
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Event işlenirken hata oluştu");
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, stoppingToken);
            logger.LogInformation("RabbitMQ consumer başlatıldı: {Queue}", QueueName);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) { /* Graceful shutdown */ }
        catch (Exception ex)
        {
            logger.LogError(ex, "RabbitMQ bağlantı hatası");
        }
    }

    private record TenderStatusChangedEvent(
        string EventType,
        Guid TenderId,
        string Title,
        string NewStatus,
        DateTime OccurredAt
    );
}
