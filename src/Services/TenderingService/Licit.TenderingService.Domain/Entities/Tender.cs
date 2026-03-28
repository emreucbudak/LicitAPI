using Licit.TenderingService.Domain.Common;
using Licit.TenderingService.Domain.Exceptions;

namespace Licit.TenderingService.Domain.Entities;

public class Tender : BaseEntity
{
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal StartingPrice { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public TenderStatus Status { get; private set; } = TenderStatus.Draft;
    public Guid CreatedByUserId { get; private set; }
    public Guid CategoryId { get; private set; }

    public Category Category { get; private set; } = null!;
    public ICollection<TenderRule> Rules { get; private set; } = new List<TenderRule>();

    private Tender() { }

    public Tender(string title, string description, decimal startingPrice,
        DateTime startDate, DateTime endDate, Guid createdByUserId, Guid categoryId)
    {
        Title = title;
        Description = description;
        StartingPrice = startingPrice;
        StartDate = startDate;
        EndDate = endDate;
        CreatedByUserId = createdByUserId;
        CategoryId = categoryId;
        Status = TenderStatus.Draft;
    }

    public void UpdateDetails(string title, string description, decimal startingPrice,
        DateTime startDate, DateTime endDate, Guid categoryId)
    {
        if (Status != TenderStatus.Draft)
            throw new TenderNotEditableException();

        Title = title;
        Description = description;
        StartingPrice = startingPrice;
        StartDate = startDate;
        EndDate = endDate;
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRule(string title, string description, bool isRequired)
    {
        Rules.Add(new TenderRule
        {
            TenderId = Id,
            Title = title,
            Description = description,
            IsRequired = isRequired
        });
    }

    public void ClearRules()
    {
        Rules.Clear();
    }

    public void ChangeStatus(TenderStatus newStatus)
    {
        var allowed = Status switch
        {
            TenderStatus.Draft => newStatus is TenderStatus.Active or TenderStatus.Cancelled,
            TenderStatus.Active => newStatus is TenderStatus.Closed or TenderStatus.Cancelled,
            TenderStatus.Closed => newStatus is TenderStatus.Completed,
            _ => false
        };

        if (!allowed)
            throw new InvalidStatusTransitionException(Status.ToString(), newStatus.ToString());

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ValidateForDeletion()
    {
        if (Status == TenderStatus.Active)
            throw new ActiveTenderDeletionException();
    }
}
