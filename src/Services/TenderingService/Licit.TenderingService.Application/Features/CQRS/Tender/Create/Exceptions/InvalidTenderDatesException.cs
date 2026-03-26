using Licit.TenderingService.Application.Exceptions;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Create.Exceptions;

public class InvalidTenderDatesException : BusinessRuleException
{
    public InvalidTenderDatesException()
        : base("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.") { }
}
