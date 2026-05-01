using FlashMediator;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Delete;

public record DeleteTenderCommandRequest(
    Guid Id
) : IRequest;
