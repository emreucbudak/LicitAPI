using FluentValidation.TestHelper;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deduct;
using Licit.WalletService.Application.Validators.Wallet.Commands.Deduct;

namespace Licit.WalletService.UnitTests.Application.Validators;

public class DeductFundsCommandValidatorTests
{
    private readonly DeductFundsCommandValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new DeductFundsCommandRequest(Guid.NewGuid(), 100, Guid.NewGuid(), null));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task UserId_WhenEmpty_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new DeductFundsCommandRequest(Guid.Empty, 100, Guid.NewGuid(), null));
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
        var result = await _validator.TestValidateAsync(new DeductFundsCommandRequest(Guid.NewGuid(), 100, Guid.Empty, null));
        result.ShouldHaveValidationErrorFor(x => x.ReferenceId);
    }
}
