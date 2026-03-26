using FluentValidation;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Update;

public class UpdateTenderCommandValidator : AbstractValidator<UpdateTenderCommandRequest>
{
    public UpdateTenderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("İhale kimliği belirtilmelidir.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("İhale başlığı boş olamaz.")
            .MaximumLength(200).WithMessage("İhale başlığı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("İhale açıklaması boş olamaz.")
            .MaximumLength(2000).WithMessage("İhale açıklaması en fazla 2000 karakter olabilir.");

        RuleFor(x => x.StartingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Başlangıç fiyatı negatif olamaz.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
    }
}
