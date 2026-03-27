using FluentValidation.TestHelper;
using Licit.WalletService.Application.Features.CQRS.Wallet.Deduct;

namespace Licit.WalletService.UnitTests.Application.Validators;

public class DeductFundsCommandValidatorTests
{
    private readonly DeductFundsCommandValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new DeductFundsCommandRequest(Guid.NewGuid(), 100m, Guid.NewGuid(), null));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task UserId_WhenEmpty_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new DeductFundsCommandRequest(Guid.Empty, 100m, Guid.NewGuid(), null));
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public async Task Amount_WhenZero_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new DeductFundsCommandRequest(Guid.NewGuid(), 0, Guid.NewGuid(), null));
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public async Task ReferenceId_WhenEmpty_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new DeductFundsCommandRequest(Guid.NewGuid(), 100m, Guid.Empty, null));
        result.ShouldHaveValidationErrorFor(x => x.ReferenceId);
    }
}
