using Licit.AuthService.Domain.Entities;

namespace Licit.AuthService.Application.Interfaces;

public interface IPasswordHistoryRepository
{
    Task<IReadOnlyList<PasswordHistory>> GetLatestByUserIdAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken = default);

    Task AddAsync(PasswordHistory passwordHistory, CancellationToken cancellationToken = default);

    void RemoveRange(IEnumerable<PasswordHistory> passwordHistories);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
