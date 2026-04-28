using FlashMediator;
using Licit.TenderingService.Application.Features.CQRS.Category.GetTree;
using Microsoft.AspNetCore.Mvc;

namespace Licit.TenderingService.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTree()
    {
        var result = await mediator.Send(new GetCategoryTreeQueryRequest());
        return Ok(result);
    }
}
