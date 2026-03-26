using FlashMediator;
using Licit.MailService.Application.Features.CQRS.Email.GetAll;
using Licit.MailService.Application.Features.CQRS.Email.GetById;
using Licit.MailService.Application.Features.CQRS.Email.Send;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Licit.MailService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailController(IMediator mediator) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendEmailRequest request)
    {
        var result = await mediator.Send(new SendEmailCommandRequest(request.To, request.Subject, request.Body));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetEmailByIdQueryRequest(id));
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetAllEmailsQueryRequest(page, pageSize));
        return Ok(result);
    }
}

public record SendEmailRequest(string To, string Subject, string Body);
