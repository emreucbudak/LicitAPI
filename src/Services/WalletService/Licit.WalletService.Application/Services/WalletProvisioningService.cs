using Licit.WalletService.Application.Extensions;
using Licit.WalletService.Application.Interfaces;
using Licit.WalletService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Services;

public class WalletProvisioningService(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork) : IWalletProvisioningService
{
    private const string WalletUserIdConstraintName = "IX_Wallets_UserId";

    public async Task<Wallet> EnsureWalletExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserIdAsync(userId);
        if (wallet is not null)
            return wallet;

        wallet = new Wallet(userId);
        walletRepository.Add(wallet);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return wallet;
        }
        catch (DbUpdateException exception) when (exception.IsUniqueConstraintViolation(WalletUserIdConstraintName))
        {
            walletRepository.Detach(wallet);

            var existingWallet = await walletRepository.GetByUserIdAsync(userId);
            if (existingWallet is not null)
                return existingWallet;

            throw;
        }
    }
}
