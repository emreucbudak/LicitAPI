using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Licit.WalletService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly WalletDbContext _context;

    public WalletRepository(WalletDbContext context) => _context = context;

    public async Task<Wallet?> GetByUserIdAsync(Guid userId) =>
        await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);

    public async Task<Wallet?> GetByUserIdForUpdateAsync(Guid userId) =>
        await _context.Wallets
            .FromSqlInterpolated($"SELECT * FROM \"Wallets\" WHERE \"UserId\" = {userId} FOR UPDATE")
            .FirstOrDefaultAsync();

    public void Add(Wallet wallet) => _context.Wallets.Add(wallet);

    public void Detach(Wallet wallet) => _context.Entry(wallet).State = EntityState.Detached;

    public void Update(Wallet wallet) => _context.Wallets.Update(wallet);

    public async Task<WalletTransaction?> GetTransactionByWalletTypeAndReferenceAsync(Guid walletId, TransactionType type, Guid referenceId) =>
        await _context.WalletTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.WalletId == walletId && t.Type == type && t.ReferenceId == referenceId);

    public async Task<IEnumerable<WalletTransaction>> GetTransactionsByWalletIdAsync(Guid walletId, int page, int pageSize) =>
        await _context.WalletTransactions
            .AsNoTracking()
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> GetTransactionCountByWalletIdAsync(Guid walletId) =>
        await _context.WalletTransactions.CountAsync(t => t.WalletId == walletId);
}
