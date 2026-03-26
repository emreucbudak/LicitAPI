namespace Licit.TenderingService.Domain.Entities;

public class TenderRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenderId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsRequired { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Tender Tender { get; set; } = null!;
}
