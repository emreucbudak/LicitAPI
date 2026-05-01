namespace Licit.AuthService.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? TokenId { get; }
}
