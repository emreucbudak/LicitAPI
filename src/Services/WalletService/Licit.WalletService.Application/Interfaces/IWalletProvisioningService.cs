using Licit.WalletService.Domain.Entities;

namespace Licit.WalletService.Application.Interfaces;

public interface IWalletProvisioningService
{
    Task<Wallet> EnsureWalletExistsAsync(Guid userId, CancellationToken cancellationToken);
}
