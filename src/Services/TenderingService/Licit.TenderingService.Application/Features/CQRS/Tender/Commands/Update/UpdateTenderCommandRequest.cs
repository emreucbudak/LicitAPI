using FlashMediator;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Update;

public record UpdateTenderCommandRequest(
    Guid Id,
    string Title,
    string Description,
    decimal StartingPrice,
    DateTime StartDate,
    DateTime EndDate,
    Guid CategoryId
) : IRequest<UpdateTenderCommandResponse>;
