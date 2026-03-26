using Licit.WalletService.Domain.Entities;

namespace Licit.WalletService.Application.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAsync(Guid userId);
    Task<Wallet> CreateAsync(Wallet wallet);
    Task UpdateAsync(Wallet wallet);
    Task<IEnumerable<WalletTransaction>> GetTransactionsByWalletIdAsync(Guid walletId, int page, int pageSize);
}
