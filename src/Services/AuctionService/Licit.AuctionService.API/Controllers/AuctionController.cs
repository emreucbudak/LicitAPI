using FlashMediator;
using Licit.AuctionService.Application.Feature.CQRS.Auction.Command.CreateAuction;
using Licit.AuctionService.Application.Feature.CQRS.Auction.Command.UpdateAuctionStatus;
using Licit.AuctionService.Application.Feature.CQRS.Auction.Command.UpdateAuctionWinnerBid;
using Licit.AuctionService.Application.Feature.CQRS.Auction.Queries.GetActiveAuctions;
using Licit.AuctionService.Application.Feature.CQRS.Auction.Queries.GetAuctionById;
using Microsoft.AspNetCore.Mvc;

namespace Licit.AuctionService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionController(IMediator mediator) : ControllerBase
    {
        [HttpGet("active")]
        public async Task<IActionResult> GetActive(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await mediator.Send(new GetActiveAuctionsQueriesRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            });

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await mediator.Send(new GetAuctionByIdQueriesRequest
            {
                AuctionId = id
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAuctionCommandRequest request)
        {
            await mediator.Send(request);
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateAuctionStatusCommandRequest request)
        {
            await mediator.Send(new UpdateAuctionStatusCommandRequest
            {
                AuctionId = id,
                Status = request.Status
            });

            return NoContent();
        }

        [HttpPatch("{id:guid}/winner-bid")]
        public async Task<IActionResult> UpdateWinnerBid(
            Guid id,
            [FromBody] UpdateAuctionWinnerBidCommandRequest request)
        {
            await mediator.Send(request with { AuctionId = id });
            return NoContent();
        }
    }
}
