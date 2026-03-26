using Licit.TenderingService.Domain.Common;

namespace Licit.TenderingService.Domain.Entities;

public class TenderRule : BaseEntity
{
    public Guid TenderId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsRequired { get; set; } = true;

    public Tender Tender { get; set; } = null!;
}
