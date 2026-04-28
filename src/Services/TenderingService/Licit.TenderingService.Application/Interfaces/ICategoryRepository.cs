using Licit.TenderingService.Domain.Entities;

namespace Licit.TenderingService.Application.Interfaces;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
}
