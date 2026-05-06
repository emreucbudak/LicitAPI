namespace Licit.AuthService.Application.Interfaces;

public interface IWalletProvisioningClient
{
    Task EnsureWalletAsync(Guid userId, CancellationToken cancellationToken);
}
