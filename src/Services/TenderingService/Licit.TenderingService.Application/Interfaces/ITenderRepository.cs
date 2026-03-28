using Licit.TenderingService.Domain.Entities;

namespace Licit.TenderingService.Application.Interfaces;

public interface ITenderRepository
{
    Task<Tender?> GetByIdAsync(Guid id);
    Task<IEnumerable<Tender>> GetAllAsync(int page, int pageSize);
    Task<int> GetCountAsync();
    Task<IEnumerable<Tender>> GetByUserIdAsync(Guid userId);
    void Add(Tender tender);
    void Update(Tender tender);
    void Remove(Tender tender);
}
