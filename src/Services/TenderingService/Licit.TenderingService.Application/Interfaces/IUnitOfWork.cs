namespace Licit.TenderingService.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ITenderRepository Tenders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
