using FluentValidation;

namespace Licit.TenderingService.Application.Features.CQRS.Tender.Create;

public class CreateTenderCommandValidator : AbstractValidator<CreateTenderCommandRequest>
{
    public CreateTenderCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("İhale başlığı boş olamaz.")
            .MaximumLength(200).WithMessage("İhale başlığı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("İhale açıklaması boş olamaz.")
            .MaximumLength(2000).WithMessage("İhale açıklaması en fazla 2000 karakter olabilir.");

        RuleFor(x => x.StartingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Başlangıç fiyatı negatif olamaz.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi belirtilmelidir.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi belirtilmelidir.")
            .GreaterThan(x => x.StartDate).WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Oluşturan kullanıcı kimliği belirtilmelidir.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori belirtilmelidir.");
    }
}
