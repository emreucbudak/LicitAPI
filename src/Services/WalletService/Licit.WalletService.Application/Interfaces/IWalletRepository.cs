using Licit.WalletService.Domain.Entities;

namespace Licit.WalletService.Application.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAsync(Guid userId);
    void Add(Wallet wallet);
    void Update(Wallet wallet);
    Task<IEnumerable<WalletTransaction>> GetTransactionsByWalletIdAsync(Guid walletId, int page, int pageSize);
    Task<int> GetTransactionCountByWalletIdAsync(Guid walletId);
}
