using Licit.TenderingService.Application.Exceptions;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Create.Exceptions;

public class InvalidStartingPriceException : BusinessRuleException
{
    public InvalidStartingPriceException()
        : base("Başlangıç fiyatı negatif olamaz.") { }
}
