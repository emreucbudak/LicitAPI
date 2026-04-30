using Licit.TenderingService.Application.Exceptions;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Create.Exceptions;

public class InvalidTenderDatesException : BusinessRuleException
{
    public InvalidTenderDatesException()
        : base("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.") { }
}
