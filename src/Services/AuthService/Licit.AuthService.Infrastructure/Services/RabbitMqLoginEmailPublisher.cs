using System.Text;
using System.Text.Json;
using Licit.AuthService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Licit.AuthService.Infrastructure.Services;

public class RabbitMqLoginEmailPublisher : ILoginEmailPublisher, IAsyncDisposable
{
    private const string ExchangeName = "licit.events";
    private const string LoginRoutingKey = "auth.login.2fa.requested";
    private const string RegisterRoutingKey = "auth.register.verification.requested";
    private const string PasswordResetRoutingKey = "auth.password-reset.requested";
    private readonly ILogger<RabbitMqLoginEmailPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private RabbitMqLoginEmailPublisher(
        ILogger<RabbitMqLoginEmailPublisher> logger,
        IConnection connection,
        IChannel channel)
    {
        _logger = logger;
        _connection = connection;
        _channel = channel;
    }

    public static async Task<RabbitMqLoginEmailPublisher> CreateAsync(
        IConfiguration configuration,
        ILogger<RabbitMqLoginEmailPublisher> logger)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMq:Port"] ?? "5672"),
            UserName = configuration["RabbitMq:Username"] ?? "licit",
            Password = configuration["RabbitMq:Password"] ?? "LicitDev2024!"
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true);

        return new RabbitMqLoginEmailPublisher(logger, connection, channel);
    }

    public async Task PublishLoginVerificationCodeAsync(
        string email,
        string code,
        DateTime expiresAt,
        string? userName,
        CancellationToken cancellationToken = default)
    {
        await PublishAsync(
            LoginRoutingKey,
            email,
            code,
            expiresAt,
            userName,
            "Auth login 2FA event published for {Email}",
            cancellationToken);
    }

    public async Task PublishRegisterVerificationCodeAsync(
        string email,
        string code,
        DateTime expiresAt,
        string? userName,
        CancellationToken cancellationToken = default)
    {
        await PublishAsync(
            RegisterRoutingKey,
            email,
            code,
            expiresAt,
            userName,
            "Auth register verification event published for {Email}",
            cancellationToken);
    }

    public async Task PublishPasswordResetCodeAsync(
        string email,
        string code,
        DateTime expiresAt,
        string? userName,
        CancellationToken cancellationToken = default)
    {
        await PublishAsync(
            PasswordResetRoutingKey,
            email,
            code,
            expiresAt,
            userName,
            "Auth password reset event published for {Email}",
            cancellationToken);
    }

    private async Task PublishAsync(
        string routingKey,
        string email,
        string code,
        DateTime expiresAt,
        string? userName,
        string logMessageTemplate,
        CancellationToken cancellationToken)
    {
        var message = new
        {
            Email = email,
            Code = code,
            ExpiresAt = expiresAt,
            UserName = userName
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        await _channel.BasicPublishAsync(ExchangeName, routingKey, body, cancellationToken);
        _logger.LogInformation(logMessageTemplate, email);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}
