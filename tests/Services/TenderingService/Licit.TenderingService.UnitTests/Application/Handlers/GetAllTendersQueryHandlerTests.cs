using FluentAssertions;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetAll;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;
using Licit.TenderingService.UnitTests.Common;
using NSubstitute;

namespace Licit.TenderingService.UnitTests.Application.Handlers;

public class GetAllTendersQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITenderRepository _tenderRepo = Substitute.For<ITenderRepository>();
    private readonly GetAllTendersQueryHandler _handler;

    public GetAllTendersQueryHandlerTests()
    {
        _unitOfWork.Tenders.Returns(_tenderRepo);
        _handler = new GetAllTendersQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WithTenders_ShouldReturnAllTenders()
    {
        var tender1 = TenderTestFactory.CreateDraftTender(title: "İhale 1");
        var tender2 = TenderTestFactory.CreateDraftTender(title: "İhale 2");

        // Category is needed for mapping - set via reflection
        SetCategory(tender1, new Category("Kategori 1"));
        SetCategory(tender2, new Category("Kategori 2"));

        _tenderRepo.GetAllAsync().Returns(new List<Tender> { tender1, tender2 });

        var result = await _handler.Handle(new GetAllTendersQueryRequest(), CancellationToken.None);

        result.Tenders.Should().HaveCount(2);
        result.Tenders[0].Title.Should().Be("İhale 1");
        result.Tenders[1].Title.Should().Be("İhale 2");
    }

    [Fact]
    public async Task Handle_WithNoTenders_ShouldReturnEmptyList()
    {
        _tenderRepo.GetAllAsync().Returns(Enumerable.Empty<Tender>());

        var result = await _handler.Handle(new GetAllTendersQueryRequest(), CancellationToken.None);

        result.Tenders.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapFieldsCorrectly()
    {
        var tender = TenderTestFactory.CreateDraftTenderWithRules(3);
        SetCategory(tender, new Category("Test Kategori"));
        _tenderRepo.GetAllAsync().Returns(new List<Tender> { tender });

        var result = await _handler.Handle(new GetAllTendersQueryRequest(), CancellationToken.None);

        var dto = result.Tenders.Single();
        dto.Id.Should().Be(tender.Id);
        dto.Status.Should().Be("Draft");
        dto.CategoryName.Should().Be("Test Kategori");
        dto.RuleCount.Should().Be(3);
    }

    private static void SetCategory(Tender tender, Category category)
    {
        var prop = typeof(Tender).GetProperty("Category")!;
        prop.SetValue(tender, category);
    }
}
