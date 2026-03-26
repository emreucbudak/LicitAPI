namespace Licit.WalletService.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IWalletRepository Wallets { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
