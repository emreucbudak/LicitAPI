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
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Tender>> GetAllAsync() =>
        await _context.Tenders
            .Include(t => t.Rules)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Tender>> GetByUserIdAsync(Guid userId) =>
        await _context.Tenders
            .Include(t => t.Rules)
            .Where(t => t.CreatedByUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<Tender> CreateAsync(Tender tender)
    {
        _context.Tenders.Add(tender);
        await _context.SaveChangesAsync();
        return tender;
    }

    public async Task UpdateAsync(Tender tender)
    {
        _context.Tenders.Update(tender);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var tender = await _context.Tenders.FindAsync(id);
        if (tender is not null)
        {
            _context.Tenders.Remove(tender);
            await _context.SaveChangesAsync();
        }
    }
}
