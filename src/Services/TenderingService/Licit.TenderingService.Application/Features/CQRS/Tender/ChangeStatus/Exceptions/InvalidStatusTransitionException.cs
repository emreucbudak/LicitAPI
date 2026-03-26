using Licit.TenderingService.Application.Exceptions;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.ChangeStatus.Exceptions;

public class InvalidStatusTransitionException : BusinessRuleException
{
    public InvalidStatusTransitionException(string currentStatus, string newStatus)
        : base($"'{currentStatus}' durumundan '{newStatus}' durumuna geçiş yapılamaz.") { }
}
