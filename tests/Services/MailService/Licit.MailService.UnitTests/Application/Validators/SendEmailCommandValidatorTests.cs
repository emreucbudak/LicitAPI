using FluentValidation.TestHelper;
using Licit.MailService.Application.Features.CQRS.Email.Send;

namespace Licit.MailService.UnitTests.Application.Validators;

public class SendEmailCommandValidatorTests
{
    private readonly SendEmailCommandValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new SendEmailCommandRequest("test@test.com", "Konu", "Gövde"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task To_WhenEmpty_ShouldHaveError(string? to)
    {
        var result = await _validator.TestValidateAsync(new SendEmailCommandRequest(to!, "Konu", "Gövde"));
        result.ShouldHaveValidationErrorFor(x => x.To);
    }

    [Fact]
    public async Task To_WhenInvalidEmail_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new SendEmailCommandRequest("gecersiz", "Konu", "Gövde"));
        result.ShouldHaveValidationErrorFor(x => x.To);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Subject_WhenEmpty_ShouldHaveError(string? subject)
    {
        var result = await _validator.TestValidateAsync(new SendEmailCommandRequest("test@test.com", subject!, "Gövde"));
        result.ShouldHaveValidationErrorFor(x => x.Subject);
    }

    [Fact]
    public async Task Subject_WhenExceeds500Chars_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new SendEmailCommandRequest("test@test.com", new string('A', 501), "Gövde"));
        result.ShouldHaveValidationErrorFor(x => x.Subject);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Body_WhenEmpty_ShouldHaveError(string? body)
    {
        var result = await _validator.TestValidateAsync(new SendEmailCommandRequest("test@test.com", "Konu", body!));
        result.ShouldHaveValidationErrorFor(x => x.Body);
    }
}
