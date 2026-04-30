using FlashMediator;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.ChangeStatus;

public record ChangeTenderStatusCommandRequest(
    Guid Id,
    string NewStatus
) : IRequest<ChangeTenderStatusCommandResponse>;
