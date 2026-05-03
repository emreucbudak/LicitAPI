using Licit.AuctionService.Application.Interface;

namespace Licit.AuctionService.Persistence.Data
{
    public class UnitOfWork(AuctionDbContext auc) : IUnitOfWork, IAsyncDisposable
    {
        public async ValueTask DisposeAsync() => await auc.DisposeAsync();


        public async Task<int> SaveChangesAsync()
        {
            return await auc.SaveChangesAsync();
        }
    }
}
