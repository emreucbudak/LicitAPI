using Licit.TenderingService.Application.Exceptions;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.ChangeStatus.Exceptions;

public class InvalidTenderStatusException : BusinessRuleException
{
    public InvalidTenderStatusException(string status)
        : base($"Geçersiz durum: {status}") { }
}
