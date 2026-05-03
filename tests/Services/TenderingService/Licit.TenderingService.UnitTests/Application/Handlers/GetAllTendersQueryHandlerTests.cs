using FluentAssertions;
using FluentValidation;
using Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetAll;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Application.Validators.Tender.Queries.GetAll;
using Licit.TenderingService.Domain.Entities;
using Licit.TenderingService.UnitTests.Common;
using NSubstitute;

namespace Licit.TenderingService.UnitTests.Application.Handlers;

public class GetAllTendersQueryHandlerTests
{
    private readonly ITenderRepository _tenderRepo = Substitute.For<ITenderRepository>();
    private readonly IValidator<GetAllTendersQueryRequest> _validator = new GetAllTendersQueryValidator();
    private readonly GetAllTendersQueryHandler _handler;

    public GetAllTendersQueryHandlerTests()
    {
        _handler = new GetAllTendersQueryHandler(_tenderRepo, _validator);
    }

    [Fact]
    public async Task Handle_WithTenders_ShouldReturnAllTenders()
    {
        var tender1 = TenderTestFactory.CreateDraftTender(title: "İhale 1");
        var tender2 = TenderTestFactory.CreateDraftTender(title: "İhale 2");

        // Category is needed for mapping - set via reflection
        SetCategory(tender1, new Category("Kategori 1"));
        SetCategory(tender2, new Category("Kategori 2"));

        _tenderRepo.GetAllAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(new List<Tender> { tender1, tender2 });
        _tenderRepo.GetCountAsync().Returns(2);

        var result = await _handler.Handle(new GetAllTendersQueryRequest(Page: 1, PageSize: 20), CancellationToken.None);

        result.Tenders.Should().HaveCount(2);
        result.Tenders[0].Title.Should().Be("İhale 1");
        result.Tenders[1].Title.Should().Be("İhale 2");
    }

    [Fact]
    public async Task Handle_WithNoTenders_ShouldReturnEmptyList()
    {
        _tenderRepo.GetAllAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(Enumerable.Empty<Tender>());
        _tenderRepo.GetCountAsync().Returns(0);

        var result = await _handler.Handle(new GetAllTendersQueryRequest(Page: 1, PageSize: 20), CancellationToken.None);

        result.Tenders.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapFieldsCorrectly()
    {
        var tender = TenderTestFactory.CreateDraftTender();
        SetCategory(tender, new Category("Test Kategori"));
        _tenderRepo.GetAllAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(new List<Tender> { tender });
        _tenderRepo.GetCountAsync().Returns(1);

        var result = await _handler.Handle(new GetAllTendersQueryRequest(Page: 1, PageSize: 20), CancellationToken.None);

        var dto = result.Tenders.Single();
        dto.Id.Should().Be(tender.Id);
        dto.Status.Should().Be("Draft");
        dto.CategoryName.Should().Be("Test Kategori");
    }

    [Fact]
    public async Task Handle_WithSearch_ShouldUseSearchRepositoryAndReturnPagination()
    {
        var tender = TenderTestFactory.CreateActiveTender();
        SetCategory(tender, new Category("Elektronik"));

        _tenderRepo.SearchAsync("laptop", true, null, 2, 10).Returns(new List<Tender> { tender });
        _tenderRepo.GetSearchCountAsync("laptop", true, null).Returns(21);

        var result = await _handler.Handle(
            new GetAllTendersQueryRequest(Page: 2, PageSize: 10, Search: "laptop", ActiveOnly: true),
            CancellationToken.None);

        result.Tenders.Should().HaveCount(1);
        result.TotalCount.Should().Be(21);
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeTrue();
        _ = _tenderRepo.DidNotReceive().GetAllAsync(Arg.Any<int>(), Arg.Any<int>());
        _ = _tenderRepo.DidNotReceive().GetCountAsync();
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldPassCategoryIdToSearchRepository()
    {
        var categoryId = Guid.NewGuid();
        var tender = TenderTestFactory.CreateActiveTender();
        SetCategory(tender, new Category("Elektronik"));

        _tenderRepo.SearchAsync(null, false, categoryId, 1, 20).Returns(new List<Tender> { tender });
        _tenderRepo.GetSearchCountAsync(null, false, categoryId).Returns(1);

        var result = await _handler.Handle(
            new GetAllTendersQueryRequest(Page: 1, PageSize: 20, CategoryId: categoryId),
            CancellationToken.None);

        result.Tenders.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        _ = _tenderRepo.DidNotReceive().GetAllAsync(Arg.Any<int>(), Arg.Any<int>());
        _ = _tenderRepo.DidNotReceive().GetCountAsync();
    }

    [Fact]
    public async Task Handle_WithInvalidPageSize_ShouldThrowValidationException()
    {
        var act = () => _handler.Handle(new GetAllTendersQueryRequest(Page: 1, PageSize: 0), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    private static void SetCategory(Tender tender, Category category)
    {
        var prop = typeof(Tender).GetProperty("Category")!;
        prop.SetValue(tender, category);
    }
}
