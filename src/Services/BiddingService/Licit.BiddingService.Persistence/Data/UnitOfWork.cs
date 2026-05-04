using Licit.BiddingService.Application.Interfaces;

namespace Licit.BiddingService.Persistence.Data
{
    public class UnitOfWork(BiddingDbContext db) : IUnitOfWork, IAsyncDisposable
    {
        public ValueTask DisposeAsync() => db.DisposeAsync();

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await db.SaveChangesAsync(cancellationToken);
        }
    }
}
