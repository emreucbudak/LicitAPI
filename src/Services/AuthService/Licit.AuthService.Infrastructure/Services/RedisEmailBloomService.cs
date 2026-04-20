using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Interfaces;
using StackExchange.Redis;

namespace Licit.AuthService.Infrastructure.Services;

public class RedisEmailBloomService(
    IConnectionMultiplexer connectionMultiplexer,
    AuthBloomFilterSettings settings) : IEmailBloomService
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    public async Task<bool> MayExistAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await _database.ExecuteAsync("BF.EXISTS", settings.RegisteredEmailsKey, NormalizeEmail(email));
        return (int)result == 1;
    }

    public async Task AddAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await EnsureFilterAsync(settings.RegisteredEmailsKey, settings.RegisteredEmailsCapacity);
        await _database.ExecuteAsync("BF.ADD", settings.RegisteredEmailsKey, NormalizeEmail(email));
    }

    private async Task EnsureFilterAsync(string key, long capacity)
    {
        try
        {
            await _database.ExecuteAsync("BF.RESERVE", key, settings.ErrorRate, capacity);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("exists", StringComparison.OrdinalIgnoreCase))
        {
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
