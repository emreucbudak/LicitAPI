using Licit.BiddingService.Domain.Entities;

namespace Licit.BiddingService.Application.Repository
{
    public interface IBiddingRepository
    {
        Task CreateBid(Bid bid, CancellationToken cancellationToken = default);
    }
}
