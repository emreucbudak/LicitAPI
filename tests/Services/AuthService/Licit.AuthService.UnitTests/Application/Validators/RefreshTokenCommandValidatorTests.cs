using FluentValidation.TestHelper;
using Licit.AuthService.Application.Features.CQRS.Auth.RefreshToken;

namespace Licit.AuthService.UnitTests.Application.Validators;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new RefreshTokenCommandRequest("some-token"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task RefreshToken_WhenEmpty_ShouldHaveError(string? token)
    {
        var result = await _validator.TestValidateAsync(new RefreshTokenCommandRequest(token!));
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}
