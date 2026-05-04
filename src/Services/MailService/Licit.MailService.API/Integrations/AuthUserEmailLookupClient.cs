using System.Net.Http.Json;

namespace Licit.MailService.API.Integrations;

public sealed class AuthUserEmailLookupClient(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<AuthUserEmailLookupClient> logger)
{
    private const string ServiceKeyHeader = "x-licit-service-key";

    public async Task<IReadOnlyList<AuthUserEmailDto>> GetEmailsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var distinctUserIds = userIds
            .Where(userId => userId != Guid.Empty)
            .Distinct()
            .ToArray();

        if (distinctUserIds.Length == 0)
        {
            return [];
        }

        var serviceKey = configuration["AuthService:ServiceKey"];

        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            logger.LogWarning("AuthService service key is not configured. Bid email recipients cannot be resolved.");
            return [];
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/auth/internal/users/emails")
        {
            Content = JsonContent.Create(new AuthUserEmailLookupRequest(distinctUserIds))
        };
        request.Headers.TryAddWithoutValidation(ServiceKeyHeader, serviceKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "AuthService email lookup failed. StatusCode: {StatusCode}",
                response.StatusCode);
            return [];
        }

        var payload = await response.Content.ReadFromJsonAsync<AuthUserEmailLookupResponse>(
            cancellationToken);

        return payload?.Users ?? [];
    }
}

public sealed record AuthUserEmailLookupRequest(IReadOnlyCollection<Guid> UserIds);

public sealed record AuthUserEmailDto(
    Guid UserId,
    string Email,
    string? UserName);

public sealed record AuthUserEmailLookupResponse(IReadOnlyList<AuthUserEmailDto> Users);
