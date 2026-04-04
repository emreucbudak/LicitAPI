using FluentAssertions;
using Licit.TenderingService.Domain.Entities;
using Licit.TenderingService.Domain.Exceptions;
using Licit.TenderingService.UnitTests.Common;

namespace Licit.TenderingService.UnitTests.Domain.Entities;

public class TenderTests
{
    #region Constructor

    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var title = "Test İhale";
        var description = "Test açıklama";
        var price = 5000m;
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var tender = new Tender(title, description, price, startDate, endDate, userId, categoryId);

        tender.Title.Should().Be(title);
        tender.Description.Should().Be(description);
        tender.StartingPrice.Should().Be(price);
        tender.StartDate.Should().Be(startDate);
        tender.EndDate.Should().Be(endDate);
        tender.CreatedByUserId.Should().Be(userId);
        tender.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public void Constructor_ShouldSetStatusToDraft()
    {
        var tender = TenderTestFactory.CreateDraftTender();
        tender.Status.Should().Be(TenderStatus.Draft);
    }

    [Fact]
    public void Constructor_ShouldInitializeEmptyRulesCollection()
    {
        var tender = TenderTestFactory.CreateDraftTender();
        tender.Rules.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldGenerateId()
    {
        var tender = TenderTestFactory.CreateDraftTender();
        tender.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldSetCreatedAt()
    {
        var before = DateTime.UtcNow;
        var tender = TenderTestFactory.CreateDraftTender();
        tender.CreatedAt.Should().BeOnOrAfter(before);
    }

    #endregion

    #region UpdateDetails

    [Fact]
    public void UpdateDetails_WhenDraft_ShouldUpdateAllFields()
    {
        var tender = TenderTestFactory.CreateDraftTender();
        var newTitle = "Güncel İhale";
        var newDesc = "Güncel açıklama";
        var newPrice = 2000m;
        var newStart = DateTime.UtcNow.AddDays(5);
        var newEnd = DateTime.UtcNow.AddDays(60);
        var newCatId = Guid.NewGuid();

        tender.UpdateDetails(newTitle, newDesc, newPrice, newStart, newEnd, newCatId);

        tender.Title.Should().Be(newTitle);
        tender.Description.Should().Be(newDesc);
        tender.StartingPrice.Should().Be(newPrice);
        tender.StartDate.Should().Be(newStart);
        tender.EndDate.Should().Be(newEnd);
        tender.CategoryId.Should().Be(newCatId);
        tender.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateDetails_WhenActive_ShouldThrow()
    {
        var tender = TenderTestFactory.CreateActiveTender();

        var act = () => tender.UpdateDetails("x", "y", 1, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), Guid.NewGuid());

        act.Should().Throw<TenderNotEditableException>();
    }

    [Fact]
    public void UpdateDetails_WhenClosed_ShouldThrow()
    {
        var tender = TenderTestFactory.CreateClosedTender();

        var act = () => tender.UpdateDetails("x", "y", 1, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), Guid.NewGuid());

        act.Should().Throw<TenderNotEditableException>();
    }

    [Fact]
    public void UpdateDetails_WhenCancelled_ShouldThrow()
    {
        var tender = TenderTestFactory.CreateCancelledTender();

        var act = () => tender.UpdateDetails("x", "y", 1, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), Guid.NewGuid());

        act.Should().Throw<TenderNotEditableException>();
    }

    [Fact]
    public void UpdateDetails_WhenCompleted_ShouldThrow()
    {
        var tender = TenderTestFactory.CreateCompletedTender();

        var act = () => tender.UpdateDetails("x", "y", 1, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), Guid.NewGuid());

        act.Should().Throw<TenderNotEditableException>();
    }

    #endregion

    #region AddRule / ClearRules

    [Fact]
    public void AddRule_ShouldAddRuleToCollection()
    {
        var tender = TenderTestFactory.CreateDraftTender();

        tender.AddRule("Kural 1", "Açıklama 1", true);

        tender.Rules.Should().HaveCount(1);
        tender.Rules.First().Title.Should().Be("Kural 1");
        tender.Rules.First().Description.Should().Be("Açıklama 1");
        tender.Rules.First().IsRequired.Should().BeTrue();
    }

    [Fact]
    public void AddRule_MultipleTimes_ShouldAddAllRules()
    {
        var tender = TenderTestFactory.CreateDraftTender();

        tender.AddRule("Kural 1", "Açıklama 1", true);
        tender.AddRule("Kural 2", "Açıklama 2", false);

        tender.Rules.Should().HaveCount(2);
    }

    [Fact]
    public void ClearRules_ShouldRemoveAllRules()
    {
        var tender = TenderTestFactory.CreateDraftTenderWithRules(3);

        tender.ClearRules();

        tender.Rules.Should().BeEmpty();
    }

    #endregion

    #region ChangeStatus - Valid Transitions

    [Theory]
    [InlineData(TenderStatus.Active)]
    [InlineData(TenderStatus.Cancelled)]
    public void ChangeStatus_FromDraft_ToValidStatus_ShouldSucceed(TenderStatus newStatus)
    {
        var tender = TenderTestFactory.CreateDraftTender();

        tender.ChangeStatus(newStatus);

        tender.Status.Should().Be(newStatus);
        tender.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(TenderStatus.Closed)]
    [InlineData(TenderStatus.Cancelled)]
    public void ChangeStatus_FromActive_ToValidStatus_ShouldSucceed(TenderStatus newStatus)
    {
        var tender = TenderTestFactory.CreateActiveTender();

        tender.ChangeStatus(newStatus);

        tender.Status.Should().Be(newStatus);
    }

    [Fact]
    public void ChangeStatus_FromClosed_ToCompleted_ShouldSucceed()
    {
        var tender = TenderTestFactory.CreateClosedTender();

        tender.ChangeStatus(TenderStatus.Completed);

        tender.Status.Should().Be(TenderStatus.Completed);
    }

    #endregion

    #region ChangeStatus - Invalid Transitions

    [Theory]
    [InlineData(TenderStatus.Closed)]
    [InlineData(TenderStatus.Completed)]
    public void ChangeStatus_FromDraft_ToInvalidStatus_ShouldThrow(TenderStatus newStatus)
    {
        var tender = TenderTestFactory.CreateDraftTender();

        var act = () => tender.ChangeStatus(newStatus);

        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Theory]
    [InlineData(TenderStatus.Draft)]
    [InlineData(TenderStatus.Active)]
    [InlineData(TenderStatus.Completed)]
    public void ChangeStatus_FromActive_ToInvalidStatus_ShouldThrow(TenderStatus newStatus)
    {
        var tender = TenderTestFactory.CreateActiveTender();

        var act = () => tender.ChangeStatus(newStatus);

        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Theory]
    [InlineData(TenderStatus.Draft)]
    [InlineData(TenderStatus.Active)]
    [InlineData(TenderStatus.Cancelled)]
    public void ChangeStatus_FromClosed_ToInvalidStatus_ShouldThrow(TenderStatus newStatus)
    {
        var tender = TenderTestFactory.CreateClosedTender();

        var act = () => tender.ChangeStatus(newStatus);

        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Theory]
    [InlineData(TenderStatus.Draft)]
    [InlineData(TenderStatus.Active)]
    [InlineData(TenderStatus.Closed)]
    [InlineData(TenderStatus.Cancelled)]
    [InlineData(TenderStatus.Completed)]
    public void ChangeStatus_FromCancelled_ToAnyStatus_ShouldThrow(TenderStatus newStatus)
    {
        var tender = TenderTestFactory.CreateCancelledTender();

        var act = () => tender.ChangeStatus(newStatus);

        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Theory]
    [InlineData(TenderStatus.Draft)]
    [InlineData(TenderStatus.Active)]
    [InlineData(TenderStatus.Closed)]
    [InlineData(TenderStatus.Cancelled)]
    [InlineData(TenderStatus.Completed)]
    public void ChangeStatus_FromCompleted_ToAnyStatus_ShouldThrow(TenderStatus newStatus)
    {
        var tender = TenderTestFactory.CreateCompletedTender();

        var act = () => tender.ChangeStatus(newStatus);

        act.Should().Throw<InvalidStatusTransitionException>();
    }

    #endregion

    #region ValidateForDeletion

    [Fact]
    public void ValidateForDeletion_WhenActive_ShouldThrow()
    {
        var tender = TenderTestFactory.CreateActiveTender();

        var act = () => tender.ValidateForDeletion();

        act.Should().Throw<ActiveTenderDeletionException>();
    }

    [Fact]
    public void ValidateForDeletion_WhenDraft_ShouldNotThrow()
    {
        var tender = TenderTestFactory.CreateDraftTender();

        var act = () => tender.ValidateForDeletion();

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateForDeletion_WhenClosed_ShouldNotThrow()
    {
        var tender = TenderTestFactory.CreateClosedTender();

        var act = () => tender.ValidateForDeletion();

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateForDeletion_WhenCancelled_ShouldNotThrow()
    {
        var tender = TenderTestFactory.CreateCancelledTender();

        var act = () => tender.ValidateForDeletion();

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateForDeletion_WhenCompleted_ShouldNotThrow()
    {
        var tender = TenderTestFactory.CreateCompletedTender();

        var act = () => tender.ValidateForDeletion();

        act.Should().NotThrow();
    }

    #endregion
}
