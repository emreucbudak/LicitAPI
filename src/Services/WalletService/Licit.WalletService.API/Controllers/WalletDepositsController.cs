using Licit.WalletService.API.Payments;
using Licit.WalletService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Licit.WalletService.API.Controllers;

[ApiController]
[Route("api/wallet/deposits")]
public class WalletDepositsController(
    StripeWalletDepositService stripeWalletDepositService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [Authorize]
    [HttpPost("payment-intents")]
    public async Task<IActionResult> CreatePaymentIntent(
        [FromBody] CreateWalletDepositPaymentIntentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is not { } userId)
            return Unauthorized();

        var result = await stripeWalletDepositService.CreatePaymentIntentAsync(
            userId,
            request.Amount,
            idempotencyKey ?? string.Empty,
            cancellationToken);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("payment-intents/{paymentIntentId}/sync")]
    public async Task<IActionResult> SyncPaymentIntent(string paymentIntentId, CancellationToken cancellationToken)
    {
        if (currentUserService.UserId is not { } userId)
            return Unauthorized();

        var result = await stripeWalletDepositService.SyncSucceededPaymentIntentAsync(
            userId,
            paymentIntentId,
            cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("stripe/webhook")]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(signature))
            return BadRequest(new { message = "Stripe imzasi bulunamadi." });

        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        try
        {
            await stripeWalletDepositService.HandleStripeWebhookAsync(payload, signature, cancellationToken);
            return Ok(new { received = true });
        }
        catch (StripeException)
        {
            return BadRequest(new { message = "Stripe webhook imzasi dogrulanamadi." });
        }
    }
}
