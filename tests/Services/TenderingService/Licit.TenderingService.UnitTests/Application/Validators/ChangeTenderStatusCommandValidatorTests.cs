using FluentValidation.TestHelper;
using Licit.TenderingService.Application.Features.CQRS.Tender.ChangeStatus;

namespace Licit.TenderingService.UnitTests.Application.Validators;

public class ChangeTenderStatusCommandValidatorTests
{
    private readonly ChangeTenderStatusCommandValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var request = new ChangeTenderStatusCommandRequest(Guid.NewGuid(), "Active");
        var result = await _validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Id_WhenEmpty_ShouldHaveError()
    {
        var request = new ChangeTenderStatusCommandRequest(Guid.Empty, "Active");
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NewStatus_WhenEmpty_ShouldHaveError(string? status)
    {
        var request = new ChangeTenderStatusCommandRequest(Guid.NewGuid(), status!);
        var result = await _validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor(x => x.NewStatus);
    }
}
