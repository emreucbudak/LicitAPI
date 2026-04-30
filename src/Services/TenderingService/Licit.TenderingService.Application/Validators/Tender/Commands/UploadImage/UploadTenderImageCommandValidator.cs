using FluentValidation;
using Licit.TenderingService.Application.Features.CQRS.Tender.Commands.UploadImage;

namespace Licit.TenderingService.Application.Validators.Tender.Commands.UploadImage;

public class UploadTenderImageCommandValidator : AbstractValidator<UploadTenderImageCommandRequest>
{
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private const long MaxImageBytes = 5 * 1024 * 1024;

    public UploadTenderImageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Ihale kimligi belirtilmelidir.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanici kimligi belirtilmelidir.");

        RuleFor(x => x.ImageStream)
            .NotNull().WithMessage("Gorsel dosyasi belirtilmelidir.");

        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("Gorsel dosyasi bos olamaz.")
            .LessThanOrEqualTo(MaxImageBytes).WithMessage("Gorsel en fazla 5 MB olabilir.");

        RuleFor(x => x.ContentType)
            .Must(contentType => AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Yalnizca JPG, PNG veya WEBP gorseller yuklenebilir.");
    }
}
