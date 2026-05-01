using FluentValidation.TestHelper;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyRegister;
using Licit.AuthService.Application.Validators.Auth.VerifyRegister;

namespace Licit.AuthService.UnitTests.Application.Validators;

public class VerifyRegisterCommandValidatorTests
{
    private readonly VerifyRegisterCommandValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new VerifyRegisterCommandRequest("test@test.com", "123456"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Email_WhenEmpty_ShouldHaveError(string? email)
    {
        var result = await _validator.TestValidateAsync(new VerifyRegisterCommandRequest(email!, "123456"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Email_WhenInvalid_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new VerifyRegisterCommandRequest("gecersiz", "123456"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("1234567")]
    public async Task Code_WhenInvalid_ShouldHaveError(string? code)
    {
        var result = await _validator.TestValidateAsync(new VerifyRegisterCommandRequest("test@test.com", code!));
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }
}
