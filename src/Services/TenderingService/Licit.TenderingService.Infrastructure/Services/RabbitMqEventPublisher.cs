using System.Text;
using System.Text.Json;
using Licit.TenderingService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Licit.TenderingService.Infrastructure.Services;

public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private const string ExchangeName = "licit.events";

    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqEventPublisher(IConfiguration configuration, ILogger<RabbitMqEventPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishTenderStatusChangedAsync(
        Guid tenderId,
        string title,
        string newStatus,
        string? imageUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var channel = await GetOrCreateChannelAsync(cancellationToken);
            if (channel is null)
                return;

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

            await channel.BasicPublishAsync(ExchangeName, routingKey, body, cancellationToken);
            _logger.LogInformation("Published TenderStatusChanged. TenderId: {TenderId}, Status: {Status}", tenderId, newStatus);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await ResetConnectionAsync();
            _logger.LogError(
                ex,
                "RabbitMQ publish failed for TenderStatusChanged. TenderId: {TenderId}, Status: {Status}. Status change has already been saved; continuing without event publish.",
                tenderId,
                newStatus);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await ResetConnectionAsync();
        _connectionLock.Dispose();
    }

    private async Task<IChannel?> GetOrCreateChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
            return _channel;

        await _connectionLock.WaitAsync(cancellationToken);
        IChannel? channel = null;
        IConnection? connection = null;
        try
        {
            if (_channel is not null)
                return _channel;

            var factory = CreateConnectionFactory(_configuration);
            connection = await factory.CreateConnectionAsync(cancellationToken);
            channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);

            _connection = connection;
            _channel = channel;
            _logger.LogInformation("RabbitMQ event publisher connected to {Host}:{Port}.", factory.HostName, factory.Port);

            return _channel;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            await CloseConnectionAsync(channel, connection);
            _logger.LogWarning(
                ex,
                "RabbitMQ event publisher is not configured correctly. TenderStatusChanged events will be skipped until configuration is fixed.");

            return null;
        }
        catch (Exception ex)
        {
            await CloseConnectionAsync(channel, connection);
            _logger.LogError(
                ex,
                "RabbitMQ connection could not be established. TenderStatusChanged events will be skipped until the connection succeeds.");

            return null;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task ResetConnectionAsync()
    {
        var channel = Interlocked.Exchange(ref _channel, null);
        var connection = Interlocked.Exchange(ref _connection, null);

        await CloseConnectionAsync(channel, connection);
    }

    private async Task CloseConnectionAsync(IChannel? channel, IConnection? connection)
    {
        if (channel is not null)
        {
            try
            {
                await channel.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "RabbitMQ channel close failed.");
            }
        }

        if (connection is not null)
        {
            try
            {
                await connection.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "RabbitMQ connection close failed.");
            }
        }
    }

    private static ConnectionFactory CreateConnectionFactory(IConfiguration configuration)
    {
        return new ConnectionFactory
        {
            HostName = RequireConfigurationValue(configuration, "RabbitMq:Host"),
            Port = RequireConfigurationInt(configuration, "RabbitMq:Port"),
            UserName = RequireConfigurationValue(configuration, "RabbitMq:Username"),
            Password = RequireConfigurationValue(configuration, "RabbitMq:Password")
        };
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
