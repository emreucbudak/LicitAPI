using DotNetCore.CAP;
using Licit.BiddingService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Licit.BiddingService.Persistence.Data
{
    public class UnitOfWork(BiddingDbContext db, ICapPublisher capPublisher) : IUnitOfWork, IAsyncDisposable
    {
        public ValueTask DisposeAsync() => db.DisposeAsync();

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> SaveChangesWithOutboxAsync(
            Func<CancellationToken, Task> publishOutboxMessagesAsync,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(publishOutboxMessagesAsync);

            await using var transaction = await db.Database.BeginTransactionAsync(
                capPublisher,
                autoCommit: false,
                cancellationToken);

            await publishOutboxMessagesAsync(cancellationToken);
            var changes = await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return changes;
        }
    }
}
