using FluentValidation.TestHelper;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetById;

namespace Licit.TenderingService.UnitTests.Application.Validators;

public class GetTenderByIdQueryValidatorTests
{
    private readonly GetTenderByIdQueryValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var request = new GetTenderByIdQueryRequest(Guid.NewGuid());
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Id_WhenEmpty_ShouldHaveError()
    {
        var request = new GetTenderByIdQueryRequest(Guid.Empty);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
