using Licit.TenderingService.Domain.Entities;

namespace Licit.TenderingService.Application.Interfaces;

public interface ITenderRepository
{
    Task<Tender?> GetByIdAsync(Guid id);
    Task<IEnumerable<Tender>> GetAllAsync();
    Task<IEnumerable<Tender>> GetByUserIdAsync(Guid userId);
    Task<Tender> CreateAsync(Tender tender);
    Task UpdateAsync(Tender tender);
    Task DeleteAsync(Guid id);
}
