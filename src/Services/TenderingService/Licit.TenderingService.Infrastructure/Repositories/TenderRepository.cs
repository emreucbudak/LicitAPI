using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;
using Licit.TenderingService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Licit.TenderingService.Infrastructure.Repositories;

public class TenderRepository : ITenderRepository
{
    private readonly TenderingDbContext _context;

    public TenderRepository(TenderingDbContext context) => _context = context;

    public async Task<Tender?> GetByIdAsync(Guid id) =>
        await _context.Tenders
            .Include(t => t.Rules)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Tender>> GetAllAsync(int page, int pageSize) =>
        await _context.Tenders
            .AsNoTracking()
            .Include(t => t.Rules)
            .Include(t => t.Category)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> GetCountAsync() =>
        await _context.Tenders.CountAsync();

    public async Task<IEnumerable<Tender>> GetByUserIdAsync(Guid userId) =>
        await _context.Tenders
            .AsNoTracking()
            .Include(t => t.Rules)
            .Include(t => t.Category)
            .Where(t => t.CreatedByUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public void Add(Tender tender) => _context.Tenders.Add(tender);

    public void Update(Tender tender) => _context.Tenders.Update(tender);

    public void Remove(Tender tender) => _context.Tenders.Remove(tender);
}
