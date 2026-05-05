namespace Licit.BiddingService.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesWithOutboxAsync(
            Func<CancellationToken, Task> publishOutboxMessagesAsync,
            CancellationToken cancellationToken = default);
    }
}
