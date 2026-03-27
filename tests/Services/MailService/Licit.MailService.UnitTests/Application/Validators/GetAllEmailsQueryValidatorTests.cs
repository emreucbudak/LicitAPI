using FluentValidation.TestHelper;
using Licit.MailService.Application.Features.CQRS.Email.GetAll;

namespace Licit.MailService.UnitTests.Application.Validators;

public class GetAllEmailsQueryValidatorTests
{
    private readonly GetAllEmailsQueryValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new GetAllEmailsQueryRequest(1, 20));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Page_WhenNotPositive_ShouldHaveError(int page)
    {
        var result = await _validator.TestValidateAsync(new GetAllEmailsQueryRequest(page, 20));
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task PageSize_WhenOutOfRange_ShouldHaveError(int pageSize)
    {
        var result = await _validator.TestValidateAsync(new GetAllEmailsQueryRequest(1, pageSize));
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}
