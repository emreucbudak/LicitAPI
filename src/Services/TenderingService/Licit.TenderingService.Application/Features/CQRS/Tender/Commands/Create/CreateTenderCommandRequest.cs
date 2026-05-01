using FlashMediator;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Create;

public record CreateTenderCommandRequest(
    string Title,
    string Description,
    decimal StartingPrice,
    DateTime StartDate,
    DateTime EndDate,
    Guid CategoryId,
    List<CreateTenderRuleDto>? Rules
) : IRequest<CreateTenderCommandResponse>;

public record CreateTenderRuleDto(
    string Title,
    string Description,
    bool IsRequired
);
