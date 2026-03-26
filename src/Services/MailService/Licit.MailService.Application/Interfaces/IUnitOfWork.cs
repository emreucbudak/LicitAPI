namespace Licit.MailService.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IEmailRepository Emails { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
