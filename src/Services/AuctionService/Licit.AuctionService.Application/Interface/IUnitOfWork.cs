namespace Licit.AuctionService.Application.Interface
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}
