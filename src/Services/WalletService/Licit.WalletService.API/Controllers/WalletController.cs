using System.Security.Claims;
using FlashMediator;
using Licit.WalletService.Application.Features.CQRS.Wallet.Deduct;
using Licit.WalletService.Application.Features.CQRS.Wallet.Deposit;
using Licit.WalletService.Application.Features.CQRS.Wallet.Freeze;
using Licit.WalletService.Application.Features.CQRS.Wallet.GetBalance;
using Licit.WalletService.Application.Features.CQRS.Wallet.GetTransactions;
using Licit.WalletService.Application.Features.CQRS.Wallet.Unfreeze;
using Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licit.WalletService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController(IMediator mediator) : ControllerBase
{
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new GetBalanceQueryRequest(userId));
        return Ok(result);
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new GetTransactionsQueryRequest(userId, page, pageSize));
        return Ok(result);
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new DepositFundsCommandRequest(userId, request.Amount));
        return Ok(result);
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new WithdrawFundsCommandRequest(userId, request.Amount));
        return Ok(result);
    }

    [HttpPost("freeze")]
    public async Task<IActionResult> Freeze([FromBody] FreezeRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new FreezeFundsCommandRequest(userId, request.Amount, request.ReferenceId, request.Description));
        return Ok(result);
    }

    [HttpPost("unfreeze")]
    public async Task<IActionResult> Unfreeze([FromBody] UnfreezeRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new UnfreezeFundsCommandRequest(userId, request.Amount, request.ReferenceId, request.Description));
        return Ok(result);
    }

    [HttpPost("deduct")]
    public async Task<IActionResult> Deduct([FromBody] DeductRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await mediator.Send(new DeductFundsCommandRequest(userId, request.Amount, request.ReferenceId, request.Description));
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value
                  ?? throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");

        return Guid.Parse(sub);
    }
}

public record DepositRequest(decimal Amount);
public record WithdrawRequest(decimal Amount);
public record FreezeRequest(decimal Amount, Guid ReferenceId, string? Description);
public record UnfreezeRequest(decimal Amount, Guid ReferenceId, string? Description);
public record DeductRequest(decimal Amount, Guid ReferenceId, string? Description);
