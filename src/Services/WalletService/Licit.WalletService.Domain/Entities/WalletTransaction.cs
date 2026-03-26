namespace Licit.WalletService.Domain.Entities;

public class WalletTransaction : BaseEntity
{
    public Guid WalletId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public Guid? ReferenceId { get; set; }
    public decimal BalanceAfter { get; set; }
    public decimal FrozenBalanceAfter { get; set; }

    public Wallet Wallet { get; set; } = null!;
}
