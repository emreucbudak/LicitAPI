using FluentAssertions;
using Licit.WalletService.Domain.Entities;
using Licit.WalletService.UnitTests.Common;

namespace Licit.WalletService.UnitTests.Domain.Entities;

public class WalletTests
{
    #region Constructor

    [Fact]
    public void Constructor_ShouldInitializeWithZeroBalances()
    {
        var userId = Guid.NewGuid();
        var wallet = new Wallet(userId);

        wallet.UserId.Should().Be(userId);
        wallet.Balance.Should().Be(0);
        wallet.FrozenBalance.Should().Be(0);
        wallet.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldInitializeEmptyTransactions()
    {
        var wallet = WalletTestFactory.CreateEmptyWallet();
        wallet.Transactions.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region Deposit

    [Fact]
    public void Deposit_ValidAmount_ShouldIncreaseBalance()
    {
        var wallet = WalletTestFactory.CreateEmptyWallet();

        var tx = wallet.Deposit(500m);

        wallet.Balance.Should().Be(500m);
        tx.Type.Should().Be(TransactionType.Deposit);
        tx.Amount.Should().Be(500m);
        tx.BalanceAfter.Should().Be(500m);
    }

    [Fact]
    public void Deposit_ShouldAddTransaction()
    {
        var wallet = WalletTestFactory.CreateEmptyWallet();

        wallet.Deposit(100m);

        wallet.Transactions.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Deposit_InvalidAmount_ShouldThrow(decimal amount)
    {
        var wallet = WalletTestFactory.CreateEmptyWallet();

        var act = () => wallet.Deposit(amount);

        act.Should().Throw<InvalidOperationException>().WithMessage("INVALID_DEPOSIT_AMOUNT");
    }

    #endregion

    #region Withdraw

    [Fact]
    public void Withdraw_ValidAmount_ShouldDecreaseBalance()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(1000m);

        var tx = wallet.Withdraw(300m);

        wallet.Balance.Should().Be(700m);
        tx.Type.Should().Be(TransactionType.Withdraw);
        tx.Amount.Should().Be(300m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Withdraw_InvalidAmount_ShouldThrow(decimal amount)
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(1000m);

        var act = () => wallet.Withdraw(amount);

        act.Should().Throw<InvalidOperationException>().WithMessage("INVALID_WITHDRAW_AMOUNT");
    }

    [Fact]
    public void Withdraw_InsufficientBalance_ShouldThrow()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(100m);

        var act = () => wallet.Withdraw(200m);

        act.Should().Throw<InvalidOperationException>().WithMessage("INSUFFICIENT_BALANCE");
    }

    #endregion

    #region Freeze

    [Fact]
    public void Freeze_ValidAmount_ShouldMoveFundsToFrozen()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(1000m);
        var refId = Guid.NewGuid();

        var tx = wallet.Freeze(300m, refId, "Test bloke");

        wallet.Balance.Should().Be(700m);
        wallet.FrozenBalance.Should().Be(300m);
        tx.Type.Should().Be(TransactionType.Freeze);
        tx.ReferenceId.Should().Be(refId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Freeze_InvalidAmount_ShouldThrow(decimal amount)
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(1000m);

        var act = () => wallet.Freeze(amount, Guid.NewGuid(), null);

        act.Should().Throw<InvalidOperationException>().WithMessage("INVALID_FREEZE_AMOUNT");
    }

    [Fact]
    public void Freeze_InsufficientBalance_ShouldThrow()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(100m);

        var act = () => wallet.Freeze(200m, Guid.NewGuid(), null);

        act.Should().Throw<InvalidOperationException>().WithMessage("INSUFFICIENT_BALANCE_FOR_FREEZE");
    }

    #endregion

    #region Unfreeze

    [Fact]
    public void Unfreeze_ValidAmount_ShouldMoveFundsBackToBalance()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(700m, 300m);
        var refId = Guid.NewGuid();

        var tx = wallet.Unfreeze(300m, refId, "Test çözme");

        wallet.Balance.Should().Be(1000m);
        wallet.FrozenBalance.Should().Be(0m);
        tx.Type.Should().Be(TransactionType.Unfreeze);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Unfreeze_InvalidAmount_ShouldThrow(decimal amount)
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500m, 500m);

        var act = () => wallet.Unfreeze(amount, Guid.NewGuid(), null);

        act.Should().Throw<InvalidOperationException>().WithMessage("INVALID_UNFREEZE_AMOUNT");
    }

    [Fact]
    public void Unfreeze_InsufficientFrozenBalance_ShouldThrow()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500m, 100m);

        var act = () => wallet.Unfreeze(200m, Guid.NewGuid(), null);

        act.Should().Throw<InvalidOperationException>().WithMessage("INSUFFICIENT_FROZEN_BALANCE");
    }

    #endregion

    #region Deduct

    [Fact]
    public void Deduct_ValidAmount_ShouldDecreaseFrozenBalance()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500m, 300m);
        var refId = Guid.NewGuid();

        var tx = wallet.Deduct(300m, refId, "İhale kesildi");

        wallet.Balance.Should().Be(500m);
        wallet.FrozenBalance.Should().Be(0m);
        tx.Type.Should().Be(TransactionType.Deduct);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Deduct_InvalidAmount_ShouldThrow(decimal amount)
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500m, 500m);

        var act = () => wallet.Deduct(amount, Guid.NewGuid(), null);

        act.Should().Throw<InvalidOperationException>().WithMessage("INVALID_DEDUCT_AMOUNT");
    }

    [Fact]
    public void Deduct_InsufficientFrozenBalance_ShouldThrow()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500m, 100m);

        var act = () => wallet.Deduct(200m, Guid.NewGuid(), null);

        act.Should().Throw<InvalidOperationException>().WithMessage("INSUFFICIENT_FROZEN_BALANCE_FOR_DEDUCT");
    }

    #endregion

    #region Transaction Tracking

    [Fact]
    public void MultipleOperations_ShouldTrackAllTransactions()
    {
        var wallet = WalletTestFactory.CreateEmptyWallet();

        wallet.Deposit(1000m);
        wallet.Freeze(200m, Guid.NewGuid(), null);
        wallet.Unfreeze(100m, Guid.NewGuid(), null);
        wallet.Deduct(100m, Guid.NewGuid(), null);
        wallet.Withdraw(300m);

        wallet.Transactions.Should().HaveCount(5);
        wallet.Balance.Should().Be(600m);
        wallet.FrozenBalance.Should().Be(0m);
    }

    #endregion
}
