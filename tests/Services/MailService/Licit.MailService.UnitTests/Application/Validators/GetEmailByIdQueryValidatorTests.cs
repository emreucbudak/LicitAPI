using FluentValidation.TestHelper;
using Licit.MailService.Application.Features.CQRS.Email.GetById;

namespace Licit.MailService.UnitTests.Application.Validators;

public class GetEmailByIdQueryValidatorTests
{
    private readonly GetEmailByIdQueryValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new GetEmailByIdQueryRequest(Guid.NewGuid()));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Id_WhenEmpty_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new GetEmailByIdQueryRequest(Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
