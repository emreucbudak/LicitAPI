using FlashMediator;
using Licit.TenderingService.Application.Features.CQRS.Tender.Commands.ChangeStatus;
using Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Create;
using Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Delete;
using Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Update;
using Licit.TenderingService.Application.Features.CQRS.Tender.Commands.UploadImage;
using Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetAll;
using Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licit.TenderingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenderController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool activeOnly = false,
        [FromQuery] Guid? categoryId = null)
    {
        var result = await mediator.Send(new GetAllTendersQueryRequest(page, pageSize, search, activeOnly, categoryId));
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
    public async Task<IActionResult> Create([FromBody] CreateTenderCommandRequest request)
    {
        var result = await mediator.Send(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTenderCommandRequest request)
    {
        var result = await mediator.Send(request with { Id = id });
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/image")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
    {
        if (file is null)
            return BadRequest(new { message = "Gorsel dosyasi belirtilmelidir." });

        await using var stream = file.OpenReadStream();

        var result = await mediator.Send(new UploadTenderImageCommandRequest(
            id,
            stream,
            file.FileName,
            file.ContentType,
            file.Length
        ));

        return Ok(result);
    }

    [Authorize]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeTenderStatusCommandRequest request)
    {
        var result = await mediator.Send(request with { Id = id });
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteTenderCommandRequest(id));
        return NoContent();
    }
}
