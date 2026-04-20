using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Licit.AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Licit.AuthService.Infrastructure.Repositories;

public class PasswordHistoryRepository(AuthDbContext context) : IPasswordHistoryRepository
{
    public async Task<IReadOnlyList<PasswordHistory>> GetLatestByUserIdAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken = default) =>
        await context.PasswordHistories
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task AddAsync(PasswordHistory passwordHistory, CancellationToken cancellationToken = default) =>
        context.PasswordHistories.AddAsync(passwordHistory, cancellationToken).AsTask();

    public void RemoveRange(IEnumerable<PasswordHistory> passwordHistories) =>
        context.PasswordHistories.RemoveRange(passwordHistories);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
