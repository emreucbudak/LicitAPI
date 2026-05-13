using FluentAssertions;
using Licit.WalletService.Domain.Entities;
using Licit.WalletService.Domain.Exceptions;
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

        var tx = wallet.Deposit(500);

        wallet.Balance.Should().Be(500);
        tx.Type.Should().Be(TransactionType.Deposit);
        tx.Amount.Should().Be(500);
        tx.BalanceAfter.Should().Be(500);
    }

    [Fact]
    public void Deposit_ShouldAddTransaction()
    {
        var wallet = WalletTestFactory.CreateEmptyWallet();

        wallet.Deposit(100);

        wallet.Transactions.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Deposit_InvalidAmount_ShouldThrow(int amount)
    {
        var wallet = WalletTestFactory.CreateEmptyWallet();

        var act = () => wallet.Deposit(amount);

        act.Should().Throw<InvalidAmountException>();
    }

    #endregion

    #region Withdraw

    [Fact]
    public void Withdraw_ValidAmount_ShouldDecreaseBalance()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(1000);

        var tx = wallet.Withdraw(300);

        wallet.Balance.Should().Be(700);
        tx.Type.Should().Be(TransactionType.Withdraw);
        tx.Amount.Should().Be(300);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Withdraw_InvalidAmount_ShouldThrow(int amount)
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(1000);

        var act = () => wallet.Withdraw(amount);

        act.Should().Throw<InvalidAmountException>();
    }

    [Fact]
    public void Withdraw_InsufficientBalance_ShouldThrow()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(100);

        var act = () => wallet.Withdraw(200);

        act.Should().Throw<InsufficientBalanceException>();
    }

    #endregion

    #region Freeze

    [Fact]
    public void Freeze_ValidAmount_ShouldMoveFundsToFrozen()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(1000);
        var refId = Guid.NewGuid();

        var tx = wallet.Freeze(300, refId, "Test bloke");

        wallet.Balance.Should().Be(700);
        wallet.FrozenBalance.Should().Be(300);
        tx.Type.Should().Be(TransactionType.Freeze);
        tx.ReferenceId.Should().Be(refId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Freeze_InvalidAmount_ShouldThrow(int amount)
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(1000);

        var act = () => wallet.Freeze(amount, Guid.NewGuid(), null);

        act.Should().Throw<InvalidAmountException>();
    }

    [Fact]
    public void Freeze_InsufficientBalance_ShouldThrow()
    {
        var wallet = WalletTestFactory.CreateWalletWithBalance(100);

        var act = () => wallet.Freeze(200, Guid.NewGuid(), null);

        act.Should().Throw<InsufficientBalanceException>();
    }

    #endregion

    #region Unfreeze

    [Fact]
    public void Unfreeze_ValidAmount_ShouldMoveFundsBackToBalance()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(700, 300);
        var refId = Guid.NewGuid();

        var tx = wallet.Unfreeze(300, refId, "Test çözme");

        wallet.Balance.Should().Be(1000);
        wallet.FrozenBalance.Should().Be(0);
        tx.Type.Should().Be(TransactionType.Unfreeze);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Unfreeze_InvalidAmount_ShouldThrow(int amount)
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500, 500);

        var act = () => wallet.Unfreeze(amount, Guid.NewGuid(), null);

        act.Should().Throw<InvalidAmountException>();
    }

    [Fact]
    public void Unfreeze_InsufficientFrozenBalance_ShouldThrow()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500, 100);

        var act = () => wallet.Unfreeze(200, Guid.NewGuid(), null);

        act.Should().Throw<InsufficientFrozenBalanceException>();
    }

    #endregion

    #region Deduct

    [Fact]
    public void Deduct_ValidAmount_ShouldDecreaseFrozenBalance()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500, 300);
        var refId = Guid.NewGuid();

        var tx = wallet.Deduct(300, refId, "İhale kesildi");

        wallet.Balance.Should().Be(500);
        wallet.FrozenBalance.Should().Be(0);
        tx.Type.Should().Be(TransactionType.Deduct);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Deduct_InvalidAmount_ShouldThrow(int amount)
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500, 500);

        var act = () => wallet.Deduct(amount, Guid.NewGuid(), null);

        act.Should().Throw<InvalidAmountException>();
    }

    [Fact]
    public void Deduct_InsufficientFrozenBalance_ShouldThrow()
    {
        var wallet = WalletTestFactory.CreateWalletWithFrozenBalance(500, 100);

        var act = () => wallet.Deduct(200, Guid.NewGuid(), null);

        act.Should().Throw<InsufficientFrozenBalanceException>();
    }

    #endregion

    #region Transaction Tracking

    [Fact]
    public void MultipleOperations_ShouldTrackAllTransactions()
    {
        var wallet = WalletTestFactory.CreateEmptyWallet();

        wallet.Deposit(1000);
        wallet.Freeze(200, Guid.NewGuid(), null);
        wallet.Unfreeze(100, Guid.NewGuid(), null);
        wallet.Deduct(100, Guid.NewGuid(), null);
        wallet.Withdraw(300);

        wallet.Transactions.Should().HaveCount(5);
        wallet.Balance.Should().Be(600);
        wallet.FrozenBalance.Should().Be(0);
    }

    #endregion
}
