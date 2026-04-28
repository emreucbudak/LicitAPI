namespace Licit.TenderingService.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    ITenderRepository Tenders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
