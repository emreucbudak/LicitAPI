using FlashMediator;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deduct;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Freeze;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Unfreeze;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Withdraw;
using Licit.WalletService.Application.Features.CQRS.Wallet.Queries.GetBalance;
using Licit.WalletService.Application.Features.CQRS.Wallet.Queries.GetTransactions;
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
        var result = await mediator.Send(new GetCurrentUserBalanceQueryRequest());
        return Ok(result);
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetCurrentUserTransactionsQueryRequest(page, pageSize));
        return Ok(result);
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(
        [FromBody] DepositCurrentUserFundsCommandRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        var result = await mediator.Send(request with { IdempotencyKey = idempotencyKey });
        return Ok(result);
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawCurrentUserFundsCommandRequest request)
    {
        var result = await mediator.Send(request);
        return Ok(result);
    }

    [HttpPost("freeze")]
    public async Task<IActionResult> Freeze([FromBody] FreezeCurrentUserFundsCommandRequest request)
    {
        var result = await mediator.Send(request);
        return Ok(result);
    }

    [HttpPost("unfreeze")]
    public async Task<IActionResult> Unfreeze([FromBody] UnfreezeCurrentUserFundsCommandRequest request)
    {
        var result = await mediator.Send(request);
        return Ok(result);
    }

    [HttpPost("deduct")]
    public async Task<IActionResult> Deduct([FromBody] DeductCurrentUserFundsCommandRequest request)
    {
        var result = await mediator.Send(request);
        return Ok(result);
    }
}
