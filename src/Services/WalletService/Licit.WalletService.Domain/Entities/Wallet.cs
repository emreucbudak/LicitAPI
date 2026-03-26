namespace Licit.WalletService.Domain.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public decimal FrozenBalance { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}
