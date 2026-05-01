using FlashMediator;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.ChangeStatus;

public record ChangeTenderStatusCommandRequest(
    Guid Id,
    string Status
) : IRequest<ChangeTenderStatusCommandResponse>;
