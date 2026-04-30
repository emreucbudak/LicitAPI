using Licit.TenderingService.Application.Interfaces;

namespace Licit.TenderingService.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly TenderingDbContext _context;

    public UnitOfWork(TenderingDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
