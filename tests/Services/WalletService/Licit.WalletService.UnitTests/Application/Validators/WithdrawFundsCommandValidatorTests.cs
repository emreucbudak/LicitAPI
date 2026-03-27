using FluentValidation.TestHelper;
using Licit.WalletService.Application.Features.CQRS.Wallet.Withdraw;

namespace Licit.WalletService.UnitTests.Application.Validators;

public class WithdrawFundsCommandValidatorTests
{
    private readonly WithdrawFundsCommandValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new WithdrawFundsCommandRequest(Guid.NewGuid(), 100m));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task UserId_WhenEmpty_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new WithdrawFundsCommandRequest(Guid.Empty, 100m));
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Amount_WhenNotPositive_ShouldHaveError(decimal amount)
    {
        var result = await _validator.TestValidateAsync(new WithdrawFundsCommandRequest(Guid.NewGuid(), amount));
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }
}
