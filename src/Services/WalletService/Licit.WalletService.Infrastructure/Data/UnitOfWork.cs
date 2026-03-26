using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Infrastructure.Repositories;

namespace Licit.WalletService.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly WalletDbContext _context;
    private IWalletRepository? _wallets;

    public UnitOfWork(WalletDbContext context)
    {
        _context = context;
    }

    public IWalletRepository Wallets => _wallets ??= new WalletRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
