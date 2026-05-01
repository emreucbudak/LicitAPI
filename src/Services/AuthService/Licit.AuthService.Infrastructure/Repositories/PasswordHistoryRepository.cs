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

    public async Task AddPreviousPasswordAsync(
        Guid userId,
        string? passwordHash,
        int historyLimit,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(historyLimit, 1);

        if (string.IsNullOrWhiteSpace(passwordHash))
            return;

        var historiesToRemove = await context.PasswordHistories
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Id)
            .Skip(historyLimit - 1)
            .ToListAsync(cancellationToken);

        await context.PasswordHistories.AddAsync(
            new PasswordHistory
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                PasswordHash = passwordHash
            },
            cancellationToken);

        if (historiesToRemove.Count > 0)
            context.PasswordHistories.RemoveRange(historiesToRemove);

        await context.SaveChangesAsync(cancellationToken);
    }
}
