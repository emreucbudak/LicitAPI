using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Infrastructure.Repositories;

namespace Licit.TenderingService.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly TenderingDbContext _context;
    private ITenderRepository? _tenders;

    public UnitOfWork(TenderingDbContext context)
    {
        _context = context;
    }

    public ITenderRepository Tenders => _tenders ??= new TenderRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
