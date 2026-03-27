using FluentValidation.TestHelper;
using Licit.AuthService.Application.Features.CQRS.Auth.Register;

namespace Licit.AuthService.UnitTests.Application.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new RegisterCommandRequest("test@test.com", "Password123!", "Ali", "Veli"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Email_WhenEmpty_ShouldHaveError(string? email)
    {
        var result = await _validator.TestValidateAsync(new RegisterCommandRequest(email!, "Password123!", "Ali", "Veli"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Email_WhenInvalid_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new RegisterCommandRequest("gecersiz", "Password123!", "Ali", "Veli"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Password_WhenEmpty_ShouldHaveError(string? password)
    {
        var result = await _validator.TestValidateAsync(new RegisterCommandRequest("test@test.com", password!, "Ali", "Veli"));
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Password_WhenTooShort_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new RegisterCommandRequest("test@test.com", "1234567", "Ali", "Veli"));
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Password_WhenExactly8Chars_ShouldNotHaveError()
    {
        var result = await _validator.TestValidateAsync(new RegisterCommandRequest("test@test.com", "12345678", "Ali", "Veli"));
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task FirstName_WhenEmpty_ShouldHaveError(string? firstName)
    {
        var result = await _validator.TestValidateAsync(new RegisterCommandRequest("test@test.com", "Password123!", firstName!, "Veli"));
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public async Task FirstName_WhenExceeds100Chars_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new RegisterCommandRequest("test@test.com", "Password123!", new string('A', 101), "Veli"));
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task LastName_WhenEmpty_ShouldHaveError(string? lastName)
    {
        var result = await _validator.TestValidateAsync(new RegisterCommandRequest("test@test.com", "Password123!", "Ali", lastName!));
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public async Task LastName_WhenExceeds100Chars_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new RegisterCommandRequest("test@test.com", "Password123!", "Ali", new string('A', 101)));
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }
}
