using Licit.AuthService.Domain.Entities;

namespace Licit.AuthService.Application.Interfaces;

public interface IPasswordHistoryRepository
{
    Task<IReadOnlyList<PasswordHistory>> GetLatestByUserIdAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken = default);

    Task AddPreviousPasswordAsync(
        Guid userId,
        string? passwordHash,
        int historyLimit,
        CancellationToken cancellationToken = default);
}
