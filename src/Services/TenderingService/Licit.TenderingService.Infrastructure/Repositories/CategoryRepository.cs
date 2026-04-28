using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;
using Licit.TenderingService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Licit.TenderingService.Infrastructure.Repositories;

public class CategoryRepository(TenderingDbContext context) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
}
