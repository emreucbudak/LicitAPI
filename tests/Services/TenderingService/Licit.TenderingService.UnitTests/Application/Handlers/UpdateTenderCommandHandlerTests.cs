using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.TenderingService.Application.Features.CQRS.Tender.Create;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetById.Exceptions;
using Licit.TenderingService.Application.Features.CQRS.Tender.Update;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Exceptions;
using Licit.TenderingService.Domain.Entities;
using Licit.TenderingService.UnitTests.Common;
using NSubstitute;

namespace Licit.TenderingService.UnitTests.Application.Handlers;

public class UpdateTenderCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITenderRepository _tenderRepo = Substitute.For<ITenderRepository>();
    private readonly IValidator<UpdateTenderCommandRequest> _validator = Substitute.For<IValidator<UpdateTenderCommandRequest>>();
    private readonly ITenderCacheInvalidator _cacheInvalidator = Substitute.For<ITenderCacheInvalidator>();
    private readonly UpdateTenderCommandHandler _handler;

    public UpdateTenderCommandHandlerTests()
    {
        _unitOfWork.Tenders.Returns(_tenderRepo);
        _validator.ValidateAsync(Arg.Any<UpdateTenderCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new UpdateTenderCommandHandler(_unitOfWork, _validator, _cacheInvalidator);
    }

    private UpdateTenderCommandRequest CreateValidRequest(Guid? id = null, Guid? userId = null) => new(
        Id: id ?? Guid.NewGuid(),
        Title: "Güncel İhale",
        Description: "Güncel açıklama",
        StartingPrice: 2000m,
        StartDate: DateTime.UtcNow.AddDays(5),
        EndDate: DateTime.UtcNow.AddDays(60),
        CategoryId: Guid.NewGuid(),
        Rules: null,
        UserId: userId ?? Guid.NewGuid()
    );

    [Fact]
    public async Task Handle_ValidRequest_ShouldUpdateAndReturnResponse()
    {
        var tender = TenderTestFactory.CreateDraftTender();
        var request = CreateValidRequest(tender.Id, tender.CreatedByUserId);
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.Description.Should().Be(request.Description);
        result.StartingPrice.Should().Be(request.StartingPrice);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithRules_ShouldClearAndAddNewRules()
    {
        var tender = TenderTestFactory.CreateDraftTenderWithRules(2);
        var request = CreateValidRequest(tender.Id, tender.CreatedByUserId) with
        {
            Rules = new List<CreateTenderRuleDto>
            {
                new("Yeni Kural", "Yeni Açıklama", true)
            }
        };
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);

        await _handler.Handle(request, CancellationToken.None);

        tender.Rules.Should().HaveCount(1);
        tender.Rules.First().Title.Should().Be("Yeni Kural");
    }

    [Fact]
    public async Task Handle_TenderNotFound_ShouldThrowTenderNotFoundException()
    {
        var request = CreateValidRequest();
        _tenderRepo.GetByIdAsync(request.Id).Returns((Tender?)null);

        var act = () => _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<TenderNotFoundException>();
    }

    [Fact]
    public async Task Handle_TenderNotDraft_ShouldThrowTenderNotEditableException()
    {
        var tender = TenderTestFactory.CreateActiveTender();
        var request = CreateValidRequest(tender.Id, tender.CreatedByUserId);
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);

        var act = () => _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<TenderNotEditableException>();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        _validator.ValidateAsync(Arg.Any<UpdateTenderCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Title", "Boş olamaz") }));

        var act = () => _handler.Handle(CreateValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
