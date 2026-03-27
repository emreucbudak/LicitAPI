using Licit.WalletService.Domain.Entities;

namespace Licit.WalletService.UnitTests.Common;

public static class WalletTestFactory
{
    public static Wallet CreateEmptyWallet(Guid? userId = null)
    {
        return new Wallet(userId ?? Guid.NewGuid());
    }

    public static Wallet CreateWalletWithBalance(decimal balance, Guid? userId = null)
    {
        var wallet = CreateEmptyWallet(userId);
        if (balance > 0)
            wallet.Deposit(balance);
        return wallet;
    }

    public static Wallet CreateWalletWithFrozenBalance(decimal available, decimal frozen, Guid? userId = null)
    {
        var wallet = CreateWalletWithBalance(available + frozen, userId);
        wallet.Freeze(frozen, Guid.NewGuid(), null);
        return wallet;
    }
}
