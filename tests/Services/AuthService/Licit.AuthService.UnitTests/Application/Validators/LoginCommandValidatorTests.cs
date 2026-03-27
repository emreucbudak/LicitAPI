using FluentValidation.TestHelper;
using Licit.AuthService.Application.Features.CQRS.Auth.Login;

namespace Licit.AuthService.UnitTests.Application.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new LoginCommandRequest("test@test.com", "Password123!"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Email_WhenEmpty_ShouldHaveError(string? email)
    {
        var result = await _validator.TestValidateAsync(new LoginCommandRequest(email!, "Password123!"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Email_WhenInvalid_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new LoginCommandRequest("gecersiz", "Password123!"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Password_WhenEmpty_ShouldHaveError(string? password)
    {
        var result = await _validator.TestValidateAsync(new LoginCommandRequest("test@test.com", password!));
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
