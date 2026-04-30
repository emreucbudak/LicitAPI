using Licit.WalletService.Application.Interfaces;

namespace Licit.WalletService.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly WalletDbContext _context;

    public UnitOfWork(WalletDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
