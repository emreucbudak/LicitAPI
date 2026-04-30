using Licit.MailService.Application.Interfaces;

namespace Licit.MailService.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly MailDbContext _context;

    public UnitOfWork(MailDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
