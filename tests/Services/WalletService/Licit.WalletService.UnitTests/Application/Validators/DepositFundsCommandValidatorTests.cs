using FluentValidation.TestHelper;
using Licit.WalletService.Application.Features.CQRS.Wallet.Commands.Deposit;
using Licit.WalletService.Application.Validators.Wallet.Commands.Deposit;

namespace Licit.WalletService.UnitTests.Application.Validators;

public class DepositFundsCommandValidatorTests
{
    private readonly DepositFundsCommandValidator _validator = new();

    [Fact]
    public async Task ValidRequest_ShouldNotHaveErrors()
    {
        var result = await _validator.TestValidateAsync(new DepositFundsCommandRequest(Guid.NewGuid(), 100, "deposit-key"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task UserId_WhenEmpty_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new DepositFundsCommandRequest(Guid.Empty, 100, "deposit-key"));
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Amount_WhenNotPositive_ShouldHaveError(int amount)
    {
        var result = await _validator.TestValidateAsync(new DepositFundsCommandRequest(Guid.NewGuid(), amount, "deposit-key"));
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public async Task IdempotencyKey_WhenEmpty_ShouldHaveError()
    {
        var result = await _validator.TestValidateAsync(new DepositFundsCommandRequest(Guid.NewGuid(), 100, string.Empty));
        result.ShouldHaveValidationErrorFor(x => x.IdempotencyKey);
    }
}
