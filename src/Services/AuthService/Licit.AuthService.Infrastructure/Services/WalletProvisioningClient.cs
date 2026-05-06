using System.Net.Http.Json;
using Licit.AuthService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Licit.AuthService.Infrastructure.Services;

public class WalletProvisioningClient(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<WalletProvisioningClient> logger) : IWalletProvisioningClient
{
    private const string ServiceKeyHeader = "x-licit-service-key";

    public async Task EnsureWalletAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (httpClient.BaseAddress is null)
        {
            logger.LogWarning("Wallet provisioning skipped because WalletProvisioning:BaseUrl is not configured.");
            return;
        }

        var serviceKey = configuration["WalletProvisioning:ServiceKey"];
        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            logger.LogWarning("Wallet provisioning skipped because WalletProvisioning:ServiceKey is not configured.");
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/wallet/internal/ensure")
        {
            Content = JsonContent.Create(new EnsureWalletRequest(userId))
        };
        request.Headers.Add(ServiceKeyHeader, serviceKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return;

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            $"Wallet provisioning failed with status {(int)response.StatusCode}. Response: {responseBody}");
    }

    private sealed record EnsureWalletRequest(Guid UserId);
}
