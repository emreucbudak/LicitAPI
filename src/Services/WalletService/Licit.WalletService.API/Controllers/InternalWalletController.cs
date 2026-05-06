using System.Security.Cryptography;
using System.Text;
using Licit.WalletService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licit.WalletService.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/wallet/internal")]
public class InternalWalletController(
    IWalletProvisioningService walletProvisioningService,
    IConfiguration configuration) : ControllerBase
{
    private const string ServiceKeyHeader = "x-licit-service-key";

    [HttpPost("ensure")]
    public async Task<IActionResult> EnsureWallet(
        [FromBody] EnsureWalletRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
            return Unauthorized();

        if (request.UserId == Guid.Empty)
            return BadRequest(new { message = "UserId is required." });

        var wallet = await walletProvisioningService.EnsureWalletExistsAsync(request.UserId, cancellationToken);

        return Ok(new EnsureWalletResponse(
            wallet.Id,
            wallet.UserId,
            wallet.Balance,
            wallet.FrozenBalance));
    }

    private bool IsAuthorized()
    {
        var expectedKey = configuration["InternalService:ServiceKey"]
            ?? configuration["InternalGrpc:ServiceKey"];
        var providedKey = Request.Headers[ServiceKeyHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(expectedKey) || string.IsNullOrEmpty(providedKey))
            return false;

        var expectedBytes = Encoding.UTF8.GetBytes(expectedKey);
        var providedBytes = Encoding.UTF8.GetBytes(providedKey);

        return expectedBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }
}

public sealed record EnsureWalletRequest(Guid UserId);

public sealed record EnsureWalletResponse(
    Guid WalletId,
    Guid UserId,
    decimal Balance,
    decimal FrozenBalance);
