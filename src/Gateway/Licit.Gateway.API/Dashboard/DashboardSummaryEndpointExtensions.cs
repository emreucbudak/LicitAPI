using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Licit.Gateway.API.Dashboard;

public static class DashboardSummaryEndpointExtensions
{
    private const string AuthClusterId = "auth-cluster";
    private const string WalletClusterId = "wallet-cluster";
    private const string TenderingClusterId = "tendering-cluster";
    private const string AuctionClusterId = "auction-cluster";

    private static readonly JsonElement EmptyObject = JsonDocument.Parse("{}").RootElement.Clone();

    public static IEndpointRouteBuilder MapDashboardSummaryEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/dashboard/summary", GetDashboardSummaryAsync);

        return endpoints;
    }

    private static async Task<IResult> GetDashboardSummaryAsync(
        HttpContext httpContext,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Headers.TryGetValue("Authorization", out var authorization) ||
            string.IsNullOrWhiteSpace(authorization.ToString()))
        {
            return Results.Unauthorized();
        }

        var httpClient = httpClientFactory.CreateClient();
        var errors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var authResult = await GetJsonAsync(
            httpClient,
            configuration,
            AuthClusterId,
            "/api/auth/me",
            authorization.ToString(),
            cancellationToken);

        if (!authResult.IsSuccess)
        {
            return Results.Problem(
                authResult.ErrorMessage,
                statusCode: authResult.StatusCode is 0 ? StatusCodes.Status401Unauthorized : authResult.StatusCode);
        }

        var profile = authResult.Payload;

        var balanceTask = GetJsonAsync(httpClient, configuration, WalletClusterId, "/api/wallet/balance", authorization.ToString(), cancellationToken);
        var transactionsTask = GetJsonAsync(httpClient, configuration, WalletClusterId, "/api/wallet/transactions?page=1&pageSize=20", authorization.ToString(), cancellationToken);
        var listingsTask = GetJsonAsync(httpClient, configuration, TenderingClusterId, "/api/tender?page=1&pageSize=20", authorization.ToString(), cancellationToken);
        var activeListingsTask = GetJsonAsync(httpClient, configuration, AuctionClusterId, "/api/auction/active?pageNumber=1&pageSize=20", authorization.ToString(), cancellationToken);

        await Task.WhenAll(balanceTask, transactionsTask, listingsTask, activeListingsTask);

        var balance = GetPayloadOrRecordError("walletBalance", balanceTask.Result, errors, EmptyObject);
        var transactions = GetPayloadOrRecordError("walletTransactions", transactionsTask.Result, errors, EmptyObject);
        var listings = GetPayloadOrRecordError("listings", listingsTask.Result, errors, EmptyObject);
        var activeListingsPayload = GetPayloadOrRecordError("activeAuctions", activeListingsTask.Result, errors, EmptyObject);

        var activeAuctions = ExtractItems(activeListingsPayload, "auctions");
        var recentBids = Array.Empty<JsonElement>();

        var response = new DashboardSummaryResponse(
            profile,
            balance,
            transactions,
            listings,
            activeAuctions,
            recentBids,
            new DashboardStats(
                GetTotalCount(listings),
                activeAuctions.Count,
                GetTotalCount(transactions),
                0),
            errors.Count > 0 ? errors : null);

        return Results.Ok(response);
    }

    private static JsonElement GetPayloadOrRecordError(
        string downstreamName,
        DownstreamJsonResult result,
        Dictionary<string, string> errors,
        JsonElement fallback)
    {
        if (result.IsSuccess)
        {
            return result.Payload;
        }

        errors[downstreamName] = result.ErrorMessage;
        return fallback;
    }

    private static async Task<DownstreamJsonResult> GetJsonAsync(
        HttpClient httpClient,
        IConfiguration configuration,
        string clusterId,
        string pathAndQuery,
        string authorization,
        CancellationToken cancellationToken,
        string? userId = null)
    {
        var baseAddress = GetClusterBaseAddress(configuration, clusterId);
        if (baseAddress is null)
        {
            return DownstreamJsonResult.Failure(StatusCodes.Status500InternalServerError, $"No destination address configured for {clusterId}.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseAddress, pathAndQuery.TrimStart('/')));
        request.Headers.TryAddWithoutValidation("Authorization", authorization);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            request.Headers.TryAddWithoutValidation("X-User-ID", userId);
        }

        try
        {
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return DownstreamJsonResult.Failure((int)response.StatusCode, $"{response.StatusCode} from {clusterId}.");
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);

            return DownstreamJsonResult.Success(document.RootElement.Clone());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return DownstreamJsonResult.Failure(StatusCodes.Status502BadGateway, ex.Message);
        }
    }

    private static Uri? GetClusterBaseAddress(IConfiguration configuration, string clusterId)
    {
        var destinations = configuration.GetSection($"ReverseProxy:Clusters:{clusterId}:Destinations").GetChildren();
        var address = destinations
            .Select(destination => destination.GetValue<string>("Address"))
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        return Uri.TryCreate(address, UriKind.Absolute, out var uri) ? uri : null;
    }

    private static IReadOnlyList<JsonElement> ExtractItems(JsonElement payload, string namedArray)
    {
        if (payload.ValueKind == JsonValueKind.Array)
        {
            return payload.EnumerateArray().Select(item => item.Clone()).ToArray();
        }

        if (payload.ValueKind != JsonValueKind.Object)
        {
            return Array.Empty<JsonElement>();
        }

        foreach (var propertyName in new[] { namedArray, "items", "data", "results", "values" })
        {
            if (TryGetProperty(payload, propertyName, out var property) && property.ValueKind == JsonValueKind.Array)
            {
                return property.EnumerateArray().Select(item => item.Clone()).ToArray();
            }
        }

        return Array.Empty<JsonElement>();
    }

    private static int GetTotalCount(JsonElement payload)
    {
        if (payload.ValueKind == JsonValueKind.Array)
        {
            return payload.GetArrayLength();
        }

        if (payload.ValueKind != JsonValueKind.Object)
        {
            return 0;
        }

        foreach (var propertyName in new[] { "totalListings", "totalCount", "totalItems", "total", "count" })
        {
            if (TryGetProperty(payload, propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var count))
            {
                return count;
            }
        }

        foreach (var propertyName in new[] { "items", "data", "results", "transactions", "listings", "tenders" })
        {
            if (TryGetProperty(payload, propertyName, out var property) && property.ValueKind == JsonValueKind.Array)
            {
                return property.GetArrayLength();
            }
        }

        return 0;
    }

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private sealed record DownstreamJsonResult(bool IsSuccess, int StatusCode, JsonElement Payload, string ErrorMessage)
    {
        public static DownstreamJsonResult Success(JsonElement payload) => new(true, StatusCodes.Status200OK, payload, string.Empty);

        public static DownstreamJsonResult Failure(int statusCode, string errorMessage) => new(false, statusCode, EmptyObject, errorMessage);
    }

    private sealed record DashboardSummaryResponse(
        JsonElement Profile,
        JsonElement Wallet,
        JsonElement WalletTransactions,
        JsonElement Listings,
        IReadOnlyList<JsonElement> ActiveAuctions,
        IReadOnlyList<JsonElement> RecentBids,
        DashboardStats Stats,
        [property: JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        IReadOnlyDictionary<string, string>? Errors);

    private sealed record DashboardStats(
        int TotalListings,
        int ActiveAuctions,
        int WalletTransactions,
        int RecentBids);
}
