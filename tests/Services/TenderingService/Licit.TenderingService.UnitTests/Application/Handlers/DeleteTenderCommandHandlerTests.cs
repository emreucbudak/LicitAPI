using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.TenderingService.Application.Features.CQRS.Tender.Delete;
using Licit.TenderingService.Application.Features.CQRS.Tender.Delete.Exceptions;
using Licit.TenderingService.Application.Features.CQRS.Tender.GetById.Exceptions;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;
using Licit.TenderingService.UnitTests.Common;
using NSubstitute;

namespace Licit.TenderingService.UnitTests.Application.Handlers;

public class DeleteTenderCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITenderRepository _tenderRepo = Substitute.For<ITenderRepository>();
    private readonly IValidator<DeleteTenderCommandRequest> _validator = Substitute.For<IValidator<DeleteTenderCommandRequest>>();
    private readonly DeleteTenderCommandHandler _handler;

    public DeleteTenderCommandHandlerTests()
    {
        _unitOfWork.Tenders.Returns(_tenderRepo);
        _validator.ValidateAsync(Arg.Any<DeleteTenderCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new DeleteTenderCommandHandler(_unitOfWork, _validator);
    }

    [Fact]
    public async Task Handle_DraftTender_ShouldDeleteSuccessfully()
    {
        var tender = TenderTestFactory.CreateDraftTender();
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);

        await _handler.Handle(new DeleteTenderCommandRequest(tender.Id), CancellationToken.None);

        _tenderRepo.Received(1).Remove(tender);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ClosedTender_ShouldDeleteSuccessfully()
    {
        var tender = TenderTestFactory.CreateClosedTender();
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);

        await _handler.Handle(new DeleteTenderCommandRequest(tender.Id), CancellationToken.None);

        _tenderRepo.Received(1).Remove(tender);
    }

    [Fact]
    public async Task Handle_TenderNotFound_ShouldThrowTenderNotFoundException()
    {
        var id = Guid.NewGuid();
        _tenderRepo.GetByIdAsync(id).Returns((Tender?)null);

        var act = () => _handler.Handle(new DeleteTenderCommandRequest(id), CancellationToken.None);

        await act.Should().ThrowAsync<TenderNotFoundException>();
    }

    [Fact]
    public async Task Handle_ActiveTender_ShouldThrowActiveTenderDeletionException()
    {
        var tender = TenderTestFactory.CreateActiveTender();
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);

        var act = () => _handler.Handle(new DeleteTenderCommandRequest(tender.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ActiveTenderDeletionException>();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        _validator.ValidateAsync(Arg.Any<DeleteTenderCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Id", "Boş olamaz") }));

        var act = () => _handler.Handle(new DeleteTenderCommandRequest(Guid.Empty), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
