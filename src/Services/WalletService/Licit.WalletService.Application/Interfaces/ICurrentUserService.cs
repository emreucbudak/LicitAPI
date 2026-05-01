namespace Licit.WalletService.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
