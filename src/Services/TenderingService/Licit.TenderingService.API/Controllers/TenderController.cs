using System.Security.Claims;
using FlashMediator;
using Licit.TenderingService.Application.Features.CQRS.Tender.ChangeStatus;
using Licit.TenderingService.Application.Features.CQRS.Tender.Create;
using Licit.TenderingService.Application.Features.CQRS.Tender.Delete;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetAll;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetById;
using Licit.TenderingService.Application.Features.CQRS.Tender.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licit.TenderingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenderController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetAllTendersQueryRequest(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetTenderByIdQueryRequest(id));
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenderRequest request)
    {
        var userId = GetCurrentUserId();

        var command = new CreateTenderCommandRequest(
            request.Title,
            request.Description,
            request.StartingPrice,
            request.StartDate,
            request.EndDate,
            userId,
            request.CategoryId,
            request.Rules
        );

        var result = await mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenderRequest request)
    {
        var userId = GetCurrentUserId();

        var command = new UpdateTenderCommandRequest(
            id,
            request.Title,
            request.Description,
            request.StartingPrice,
            request.StartDate,
            request.EndDate,
            request.CategoryId,
            request.Rules,
            userId
        );

        var result = await mediator.Send(command);
        return Ok(result);
    }

    [Authorize]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request)
    {
        var result = await mediator.Send(new ChangeTenderStatusCommandRequest(id, request.Status));
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        await mediator.Send(new DeleteTenderCommandRequest(id, userId));
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value
                  ?? throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");

        return Guid.Parse(sub);
    }
}

public record CreateTenderRequest(
    string Title,
    string Description,
    decimal StartingPrice,
    DateTime StartDate,
    DateTime EndDate,
    Guid CategoryId,
    List<CreateTenderRuleDto>? Rules
);

public record UpdateTenderRequest(
    string Title,
    string Description,
    decimal StartingPrice,
    DateTime StartDate,
    DateTime EndDate,
    Guid CategoryId,
    List<CreateTenderRuleDto>? Rules
);

public record ChangeStatusRequest(string Status);
