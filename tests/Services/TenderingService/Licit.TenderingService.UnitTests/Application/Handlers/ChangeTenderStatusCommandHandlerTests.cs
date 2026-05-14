using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.TenderingService.Application.Features.CQRS.Tender.Commands.ChangeStatus;
using Licit.TenderingService.Application.Features.CQRS.Tender.Commands.ChangeStatus.Exceptions;
using Licit.TenderingService.Application.Features.CQRS.Tender.Queries.GetById.Exceptions;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;
using Licit.TenderingService.UnitTests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Licit.TenderingService.UnitTests.Application.Handlers;

public class ChangeTenderStatusCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITenderRepository _tenderRepo = Substitute.For<ITenderRepository>();
    private readonly IValidator<ChangeTenderStatusCommandRequest> _validator = Substitute.For<IValidator<ChangeTenderStatusCommandRequest>>();
    private readonly ITenderCacheInvalidator _cacheInvalidator = Substitute.For<ITenderCacheInvalidator>();
    private readonly IEventPublisher _eventPublisher = Substitute.For<IEventPublisher>();
    private readonly ChangeTenderStatusCommandHandler _handler;

    public ChangeTenderStatusCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<ChangeTenderStatusCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new ChangeTenderStatusCommandHandler(_unitOfWork, _tenderRepo, _validator, _cacheInvalidator, _eventPublisher, NullLogger<ChangeTenderStatusCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_DraftToActive_ShouldChangeStatusSuccessfully()
    {
        var tender = TenderTestFactory.CreateDraftTender();
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);
        var request = new ChangeTenderStatusCommandRequest(tender.Id, "Active");

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Status.Should().Be("Active");
        result.Id.Should().Be(tender.Id);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ActiveToClosed_ShouldChangeStatusSuccessfully()
    {
        var tender = TenderTestFactory.CreateActiveTender();
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);
        var request = new ChangeTenderStatusCommandRequest(tender.Id, "Closed");

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Status.Should().Be("Closed");
    }

    [Fact]
    public async Task Handle_CaseInsensitiveStatus_ShouldWork()
    {
        var tender = TenderTestFactory.CreateDraftTender();
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);
        var request = new ChangeTenderStatusCommandRequest(tender.Id, "active");

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_TenderNotFound_ShouldThrowTenderNotFoundException()
    {
        var id = Guid.NewGuid();
        _tenderRepo.GetByIdAsync(id).Returns((Tender?)null);

        var act = () => _handler.Handle(new ChangeTenderStatusCommandRequest(id, "Active"), CancellationToken.None);

        await act.Should().ThrowAsync<TenderNotFoundException>();
    }

    [Fact]
    public async Task Handle_InvalidStatusString_ShouldThrowInvalidTenderStatusException()
    {
        var tender = TenderTestFactory.CreateDraftTender();
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);

        var act = () => _handler.Handle(new ChangeTenderStatusCommandRequest(tender.Id, "InvalidStatus"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidTenderStatusException>();
    }

    [Fact]
    public async Task Handle_InvalidTransition_ShouldThrowInvalidStatusTransitionException()
    {
        var tender = TenderTestFactory.CreateDraftTender();
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);

        var act = () => _handler.Handle(new ChangeTenderStatusCommandRequest(tender.Id, "Completed"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidStatusTransitionException>();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        _validator.ValidateAsync(Arg.Any<ChangeTenderStatusCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Id", "Boş olamaz") }));

        var act = () => _handler.Handle(new ChangeTenderStatusCommandRequest(Guid.Empty, "Active"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_EventPublisherFails_ShouldStillReturnSavedStatus()
    {
        var tender = TenderTestFactory.CreateDraftTender();
        _tenderRepo.GetByIdAsync(tender.Id).Returns(tender);
        _eventPublisher
            .PublishTenderStatusChangedAsync(tender.Id, tender.Title, "Active", tender.ImageUrl, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("RabbitMQ unavailable")));

        var result = await _handler.Handle(new ChangeTenderStatusCommandRequest(tender.Id, "Active"), CancellationToken.None);

        result.Status.Should().Be("Active");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
