namespace Licit.AuthService.Application.Interfaces;

public interface IEmailBloomService
{
    Task<bool> MayExistAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(string email, CancellationToken cancellationToken = default);
}
