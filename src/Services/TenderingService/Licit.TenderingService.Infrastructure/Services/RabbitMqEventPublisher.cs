using System.Text;
using System.Text.Json;
using Licit.TenderingService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Licit.TenderingService.Infrastructure.Services;

public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private const string ExchangeName = "licit.events";

    private RabbitMqEventPublisher(ILogger<RabbitMqEventPublisher> logger, IConnection connection, IChannel channel)
    {
        _logger = logger;
        _connection = connection;
        _channel = channel;
    }

    public static async Task<RabbitMqEventPublisher> CreateAsync(IConfiguration configuration, ILogger<RabbitMqEventPublisher> logger)
    {
        var factory = new ConnectionFactory
        {
            HostName = RequireConfigurationValue(configuration, "RabbitMq:Host"),
            Port = RequireConfigurationInt(configuration, "RabbitMq:Port"),
            UserName = RequireConfigurationValue(configuration, "RabbitMq:Username"),
            Password = RequireConfigurationValue(configuration, "RabbitMq:Password")
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true);

        return new RabbitMqEventPublisher(logger, connection, channel);
    }

    public async Task PublishTenderStatusChangedAsync(Guid tenderId, string title, string newStatus, string? imageUrl = null, CancellationToken cancellationToken = default)
    {
        var message = new
        {
            EventType = "TenderStatusChanged",
            TenderId = tenderId,
            Title = title,
            NewStatus = newStatus,
            ImageUrl = imageUrl,
            OccurredAt = DateTime.UtcNow
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var routingKey = $"tender.status.{newStatus.ToLowerInvariant()}";

        await _channel.BasicPublishAsync(ExchangeName, routingKey, body, cancellationToken);
        _logger.LogInformation("Yayınlandı: TenderStatusChanged - {TenderId} -> {Status}", tenderId, newStatus);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }

    private static string RequireConfigurationValue(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{key} must be configured.");

        return value;
    }

    private static int RequireConfigurationInt(IConfiguration configuration, string key)
    {
        var value = RequireConfigurationValue(configuration, key);
        if (!int.TryParse(value, out var parsed))
            throw new InvalidOperationException($"{key} must be a valid integer.");

        return parsed;
    }
}
