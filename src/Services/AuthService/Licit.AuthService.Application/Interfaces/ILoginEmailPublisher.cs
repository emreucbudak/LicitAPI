namespace Licit.AuthService.Application.Interfaces;

public interface ILoginEmailPublisher
{
    Task PublishLoginVerificationCodeAsync(
        string email,
        string code,
        DateTime expiresAt,
        string? userName,
        CancellationToken cancellationToken = default);
}
