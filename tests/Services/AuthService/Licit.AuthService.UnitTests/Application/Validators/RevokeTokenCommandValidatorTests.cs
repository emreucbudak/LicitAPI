using FluentValidation.TestHelper;
using Licit.AuthService.Application.Features.CQRS.Auth.RevokeToken;

namespace Licit.AuthService.UnitTests.Application.Validators;

public class RevokeTokenCommandValidatorTests
{
    private readonly RevokeTokenCommandValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new RevokeTokenCommandRequest("some-token"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task RefreshToken_WhenEmpty_ShouldHaveError(string? token)
    {
        var result = await _validator.TestValidateAsync(new RevokeTokenCommandRequest(token!));
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}
