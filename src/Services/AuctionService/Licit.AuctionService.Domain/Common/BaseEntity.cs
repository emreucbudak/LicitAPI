namespace Licit.AuctionService.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public DateTime? UpdatedAt { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        protected void MarkUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
