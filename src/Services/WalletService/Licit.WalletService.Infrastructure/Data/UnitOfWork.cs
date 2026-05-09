using Licit.WalletService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Licit.WalletService.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly WalletDbContext _context;

    public UnitOfWork(WalletDbContext context)
    {
        _context = context;
    }

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction is not null)
            return NoopUnitOfWorkTransaction.Instance;

        return new EfCoreUnitOfWorkTransaction(await _context.Database.BeginTransactionAsync(cancellationToken));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();

    private sealed class EfCoreUnitOfWorkTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
    {
        public async Task CommitAsync(CancellationToken cancellationToken = default) =>
            await transaction.CommitAsync(cancellationToken);

        public async Task RollbackAsync(CancellationToken cancellationToken = default) =>
            await transaction.RollbackAsync(cancellationToken);

        public async ValueTask DisposeAsync() =>
            await transaction.DisposeAsync();
    }

    private sealed class NoopUnitOfWorkTransaction : IUnitOfWorkTransaction
    {
        public static readonly NoopUnitOfWorkTransaction Instance = new();

        private NoopUnitOfWorkTransaction() { }

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
