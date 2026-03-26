using Licit.TenderingService.Application.Exceptions;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Update.Exceptions;

public class TenderNotEditableException : BusinessRuleException
{
    public TenderNotEditableException()
        : base("Sadece taslak durumundaki ihaleler güncellenebilir.") { }
}
