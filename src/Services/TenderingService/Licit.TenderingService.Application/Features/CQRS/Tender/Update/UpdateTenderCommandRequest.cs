using FlashMediator;
using Licit.TenderingService.Application.Features.CQRS.Tender.Create;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Update;

public record UpdateTenderCommandRequest(
    Guid Id,
    string Title,
    string Description,
    decimal StartingPrice,
    DateTime StartDate,
    DateTime EndDate,
    Guid CategoryId,
    List<CreateTenderRuleDto>? Rules
) : IRequest<UpdateTenderCommandResponse>;
