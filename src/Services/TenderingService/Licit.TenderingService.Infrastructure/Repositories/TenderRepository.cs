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
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Tender>> GetAllAsync(int page, int pageSize) =>
        await _context.Tenders
            .AsNoTracking()
            .Include(t => t.Category)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> GetCountAsync() =>
        await _context.Tenders.CountAsync();

    public async Task<IEnumerable<Tender>> SearchAsync(string? search, bool activeOnly, Guid? categoryId, int page, int pageSize)
    {
        var categoryIds = await GetCategoryFilterIdsAsync(categoryId);

        return await ApplySearchFilters(_context.Tenders.AsNoTracking(), search, activeOnly, categoryIds)
            .Include(t => t.Category)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetSearchCountAsync(string? search, bool activeOnly, Guid? categoryId)
    {
        var categoryIds = await GetCategoryFilterIdsAsync(categoryId);

        return await ApplySearchFilters(_context.Tenders.AsNoTracking(), search, activeOnly, categoryIds)
            .CountAsync();
    }

    public async Task<IEnumerable<Tender>> GetByUserIdAsync(Guid userId) =>
        await _context.Tenders
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.CreatedByUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public void Add(Tender tender) => _context.Tenders.Add(tender);

    public void Update(Tender tender) => _context.Tenders.Update(tender);

    public void Remove(Tender tender) => _context.Tenders.Remove(tender);

    private async Task<IReadOnlyCollection<Guid>?> GetCategoryFilterIdsAsync(Guid? categoryId)
    {
        if (!categoryId.HasValue)
            return null;

        var categories = await _context.Categories
            .AsNoTracking()
            .Select(c => new { c.Id, c.ParentCategoryId })
            .ToListAsync();

        var categoryIds = new HashSet<Guid> { categoryId.Value };
        var didAddCategory = true;

        while (didAddCategory)
        {
            didAddCategory = false;

            foreach (var category in categories)
            {
                if (category.ParentCategoryId.HasValue &&
                    categoryIds.Contains(category.ParentCategoryId.Value) &&
                    categoryIds.Add(category.Id))
                {
                    didAddCategory = true;
                }
            }
        }

        return categoryIds;
    }

    private static IQueryable<Tender> ApplySearchFilters(
        IQueryable<Tender> query,
        string? search,
        bool activeOnly,
        IReadOnlyCollection<Guid>? categoryIds)
    {
        if (activeOnly)
            query = query.Where(t => t.Status == TenderStatus.Active);

        if (categoryIds is { Count: > 0 })
            query = query.Where(t => categoryIds.Contains(t.CategoryId));

        var normalizedSearch = search?.Trim().ToLower();
        if (string.IsNullOrWhiteSpace(normalizedSearch))
            return query;

        var matchingStatuses = Enum.GetValues<TenderStatus>()
            .Where(status => status.ToString().ToLower().Contains(normalizedSearch))
            .ToArray();

        return query.Where(t =>
            t.Title.ToLower().Contains(normalizedSearch) ||
            t.Description.ToLower().Contains(normalizedSearch) ||
            matchingStatuses.Contains(t.Status));
    }
}
