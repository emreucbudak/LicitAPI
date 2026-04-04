using FluentValidation.TestHelper;
using Licit.TenderingService.Application.Features.CQRS.Tender.Delete;

namespace Licit.TenderingService.UnitTests.Application.Validators;

public class DeleteTenderCommandValidatorTests
{
    private readonly DeleteTenderCommandValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var request = new DeleteTenderCommandRequest(Guid.NewGuid(), Guid.NewGuid());
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Id_WhenEmpty_ShouldHaveError()
    {
        var request = new DeleteTenderCommandRequest(Guid.Empty, Guid.NewGuid());
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
