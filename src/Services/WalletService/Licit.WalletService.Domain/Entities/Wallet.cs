using Licit.WalletService.Domain.Common;
using Licit.WalletService.Domain.Exceptions;

namespace Licit.WalletService.Domain.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; private set; }
    public decimal Balance { get; private set; }
    public decimal FrozenBalance { get; private set; }
    public byte[] RowVersion { get; set; } = null!;

    public ICollection<WalletTransaction> Transactions { get; private set; } = new List<WalletTransaction>();

    private Wallet() { }

    public Wallet(Guid userId)
    {
        UserId = userId;
        Balance = 0;
        FrozenBalance = 0;
    }

    public WalletTransaction Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidAmountException("Deposit");

        Balance += amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction(Id, TransactionType.Deposit, amount,
            "Para yatırma", null, Balance, FrozenBalance);
        Transactions.Add(transaction);
        return transaction;
    }

    public WalletTransaction Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidAmountException("Withdraw");
        if (Balance < amount)
            throw new InsufficientBalanceException();

        Balance -= amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction(Id, TransactionType.Withdraw, amount,
            "Para çekme", null, Balance, FrozenBalance);
        Transactions.Add(transaction);
        return transaction;
    }

    public WalletTransaction Freeze(decimal amount, Guid referenceId, string? description)
    {
        if (amount <= 0)
            throw new InvalidAmountException("Freeze");
        if (Balance < amount)
            throw new InsufficientBalanceException();

        Balance -= amount;
        FrozenBalance += amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction(Id, TransactionType.Freeze, amount,
            description ?? "Teklif için bakiye bloke edildi", referenceId, Balance, FrozenBalance);
        Transactions.Add(transaction);
        return transaction;
    }

    public WalletTransaction Unfreeze(decimal amount, Guid referenceId, string? description)
    {
        if (amount <= 0)
            throw new InvalidAmountException("Unfreeze");
        if (FrozenBalance < amount)
            throw new InsufficientFrozenBalanceException();

        FrozenBalance -= amount;
        Balance += amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction(Id, TransactionType.Unfreeze, amount,
            description ?? "İhale kaybedildi, bloke çözüldü", referenceId, Balance, FrozenBalance);
        Transactions.Add(transaction);
        return transaction;
    }

    public WalletTransaction Deduct(decimal amount, Guid referenceId, string? description)
    {
        if (amount <= 0)
            throw new InvalidAmountException("Deduct");
        if (FrozenBalance < amount)
            throw new InsufficientFrozenBalanceException();

        FrozenBalance -= amount;
        UpdatedAt = DateTime.UtcNow;

        var transaction = new WalletTransaction(Id, TransactionType.Deduct, amount,
            description ?? "İhale kazanıldı, tutar kesildi", referenceId, Balance, FrozenBalance);
        Transactions.Add(transaction);
        return transaction;
    }
}
