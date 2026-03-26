namespace Licit.TenderingService.Domain.Entities;

public class Tender : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal StartingPrice { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TenderStatus Status { get; set; } = TenderStatus.Draft;
    public Guid CreatedByUserId { get; set; }

    public ICollection<TenderRule> Rules { get; set; } = new List<TenderRule>();
}
