using System.Text;
using System.Text.Json;
using Licit.BiddingService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Licit.BiddingService.Infrastructure.Services
{
    public class RabbitMqBidEmailNotificationPublisher : IBidEmailNotificationPublisher, IAsyncDisposable
    {
        private const string ExchangeName = "licit.events";
        private const string OutbidEmailRequestedRoutingKey = "bidding.bid.outbid-email.requested";
        private readonly ILogger<RabbitMqBidEmailNotificationPublisher> _logger;
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        private RabbitMqBidEmailNotificationPublisher(
            ILogger<RabbitMqBidEmailNotificationPublisher> logger,
            IConnection connection,
            IChannel channel)
        {
            _logger = logger;
            _connection = connection;
            _channel = channel;
        }

        public static async Task<RabbitMqBidEmailNotificationPublisher> CreateAsync(
            IConfiguration configuration,
            ILogger<RabbitMqBidEmailNotificationPublisher> logger)
        {
            var host = configuration["RabbitMq:Host"];
            var username = configuration["RabbitMq:Username"];
            var password = configuration["RabbitMq:Password"];

            var factory = new ConnectionFactory
            {
                HostName = string.IsNullOrWhiteSpace(host) ? "localhost" : host,
                Port = int.Parse(configuration["RabbitMq:Port"] ?? "5672"),
                UserName = string.IsNullOrWhiteSpace(username) ? "licit" : username,
                Password = string.IsNullOrWhiteSpace(password) ? "LicitDev2024!" : password
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true);

            return new RabbitMqBidEmailNotificationPublisher(logger, connection, channel);
        }

        public async Task PublishOutbidEmailRequestedAsync(
            Guid auctionId,
            Guid bidId,
            Guid newBidderUserId,
            int amount,
            DateTime placedAt,
            IReadOnlyCollection<Guid> recipientUserIds,
            CancellationToken cancellationToken = default)
        {
            if (recipientUserIds.Count == 0)
                return;

            var message = new
            {
                EventType = "BidOutbidEmailRequested",
                AuctionId = auctionId,
                BidId = bidId,
                NewBidderUserId = newBidderUserId,
                Amount = amount,
                PlacedAt = placedAt,
                RecipientUserIds = recipientUserIds,
                OccurredAt = DateTime.UtcNow
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            try
            {
                await _channel.BasicPublishAsync(ExchangeName, OutbidEmailRequestedRoutingKey, body, cancellationToken);

                _logger.LogInformation(
                    "Published bid outbid email request. AuctionId: {AuctionId}, BidId: {BidId}, RecipientCount: {RecipientCount}",
                    auctionId,
                    bidId,
                    recipientUserIds.Count);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Bid outbid email request could not be published. AuctionId: {AuctionId}, BidId: {BidId}",
                    auctionId,
                    bidId);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _channel.CloseAsync();
            await _connection.CloseAsync();
        }
    }
}
