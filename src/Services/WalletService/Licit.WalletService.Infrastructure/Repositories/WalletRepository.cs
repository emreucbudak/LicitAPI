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

    public void Add(Wallet wallet) => _context.Wallets.Add(wallet);

    public void Update(Wallet wallet) => _context.Wallets.Update(wallet);

    public async Task<IEnumerable<WalletTransaction>> GetTransactionsByWalletIdAsync(Guid walletId, int page, int pageSize) =>
        await _context.WalletTransactions
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
}
