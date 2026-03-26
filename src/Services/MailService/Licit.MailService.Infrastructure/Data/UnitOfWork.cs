using Licit.MailService.Application.Interfaces;
using Licit.MailService.Infrastructure.Repositories;

namespace Licit.MailService.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly MailDbContext _context;
    private IEmailRepository? _emails;

    public UnitOfWork(MailDbContext context)
    {
        _context = context;
    }

    public IEmailRepository Emails => _emails ??= new EmailRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
