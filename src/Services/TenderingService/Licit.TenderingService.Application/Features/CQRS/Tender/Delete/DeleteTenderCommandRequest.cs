using FlashMediator;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Delete;

public record DeleteTenderCommandRequest(
    Guid Id
) : IRequest;
