using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetById;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetById.Exceptions;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;
using Licit.TenderingService.UnitTests.Common;
using NSubstitute;

namespace Licit.TenderingService.UnitTests.Application.Handlers;

public class GetTenderByIdQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITenderRepository _tenderRepo = Substitute.For<ITenderRepository>();
    private readonly IValidator<GetTenderByIdQueryRequest> _validator = Substitute.For<IValidator<GetTenderByIdQueryRequest>>();
    private readonly GetTenderByIdQueryHandler _handler;

    public GetTenderByIdQueryHandlerTests()
    {
        _unitOfWork.Tenders.Returns(_tenderRepo);
        _validator.ValidateAsync(Arg.Any<GetTenderByIdQueryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new GetTenderByIdQueryHandler(_unitOfWork, _validator);
    }

    [Fact]
    public async Task Handle_TenderExists_ShouldReturnDetailedResponse()
    {
        var tender = TenderTestFactory.CreateDraftTenderWithRules(2);
        SetCategory(tender, new Category("Test Kategori"));
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);

        var result = await _handler.Handle(new GetTenderByIdQueryRequest(tender.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(tender.Id);
        result.Title.Should().Be(tender.Title);
        result.Status.Should().Be("Draft");
        result.CategoryName.Should().Be("Test Kategori");
        result.Rules.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_TenderNotFound_ShouldThrowTenderNotFoundException()
    {
        var id = Guid.NewGuid();
        _tenderRepo.GetByIdAsync(id).Returns((Tender?)null);

        var act = () => _handler.Handle(new GetTenderByIdQueryRequest(id), CancellationToken.None);

        await act.Should().ThrowAsync<TenderNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldMapRulesCorrectly()
    {
        var tender = TenderTestFactory.CreateDraftTender();
        tender.AddRule("Kural 1", "Açıklama 1", true);
        tender.AddRule("Kural 2", "Açıklama 2", false);
        SetCategory(tender, new Category("Kat"));
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);

        var result = await _handler.Handle(new GetTenderByIdQueryRequest(tender.Id), CancellationToken.None);

        result.Rules.Should().HaveCount(2);
        result.Rules[0].Title.Should().Be("Kural 1");
        result.Rules[0].IsRequired.Should().BeTrue();
        result.Rules[1].Title.Should().Be("Kural 2");
        result.Rules[1].IsRequired.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        _validator.ValidateAsync(Arg.Any<GetTenderByIdQueryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Id", "Boş olamaz") }));

        var act = () => _handler.Handle(new GetTenderByIdQueryRequest(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    private static void SetCategory(Tender tender, Category category)
    {
        var prop = typeof(Tender).GetProperty("Category")!;
        prop.SetValue(tender, category);
    }
}
