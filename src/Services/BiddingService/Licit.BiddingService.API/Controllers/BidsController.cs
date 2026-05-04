using FlashMediator;
using Licit.BiddingService.Application.Features.CQRS.Command.CreateBidCommand;
using Microsoft.AspNetCore.Mvc;

namespace Licit.BiddingService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BidsController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateBid(
            [FromBody] CreateBidCommandRequest request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
        {
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
                request.IdempotencyKey = idempotencyKey;

            var result = await mediator.Send(request);
            return Ok(result);
        }
    }
}
