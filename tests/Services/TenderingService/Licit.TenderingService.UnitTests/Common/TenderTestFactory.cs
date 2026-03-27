using Licit.TenderingService.Domain.Entities;

namespace Licit.TenderingService.UnitTests.Common;

public static class TenderTestFactory
{
    public static Tender CreateDraftTender(
        string title = "Test İhale",
        string description = "Test açıklama",
        decimal startingPrice = 1000m,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? createdByUserId = null,
        Guid? categoryId = null)
    {
        return new Tender(
            title,
            description,
            startingPrice,
            startDate ?? DateTime.UtcNow.AddDays(1),
            endDate ?? DateTime.UtcNow.AddDays(30),
            createdByUserId ?? Guid.NewGuid(),
            categoryId ?? Guid.NewGuid()
        );
    }

    public static Tender CreateActiveTender()
    {
        var tender = CreateDraftTender();
        tender.ChangeStatus(TenderStatus.Active);
        return tender;
    }

    public static Tender CreateClosedTender()
    {
        var tender = CreateActiveTender();
        tender.ChangeStatus(TenderStatus.Closed);
        return tender;
    }

    public static Tender CreateCancelledTender()
    {
        var tender = CreateDraftTender();
        tender.ChangeStatus(TenderStatus.Cancelled);
        return tender;
    }

    public static Tender CreateCompletedTender()
    {
        var tender = CreateClosedTender();
        tender.ChangeStatus(TenderStatus.Completed);
        return tender;
    }

    public static Tender CreateDraftTenderWithRules(int ruleCount = 2)
    {
        var tender = CreateDraftTender();
        for (int i = 1; i <= ruleCount; i++)
            tender.AddRule($"Kural {i}", $"Kural açıklama {i}", true);
        return tender;
    }
}
