using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.TenderingService.Application.Features.CQRS.Tender.Commands.Create;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;
using NSubstitute;

namespace Licit.TenderingService.UnitTests.Application.Handlers;

public class CreateTenderCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITenderRepository _tenderRepo = Substitute.For<ITenderRepository>();
    private readonly IValidator<CreateTenderCommandRequest> _validator = Substitute.For<IValidator<CreateTenderCommandRequest>>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ITenderCacheInvalidator _cacheInvalidator = Substitute.For<ITenderCacheInvalidator>();
    private readonly CreateTenderCommandHandler _handler;

    public CreateTenderCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<CreateTenderCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _handler = new CreateTenderCommandHandler(_unitOfWork, _tenderRepo, _validator, _currentUserService, _cacheInvalidator);
    }

    private CreateTenderCommandRequest CreateValidRequest() => new(
        Title: "Test Ihale",
        Description: "Test aciklama",
        StartingPrice: 1000m,
        StartDate: DateTime.UtcNow.AddDays(1),
        EndDate: DateTime.UtcNow.AddDays(30),
        CategoryId: Guid.NewGuid()
    );

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateTenderAndReturnResponse()
    {
        var request = CreateValidRequest();

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.Description.Should().Be(request.Description);
        result.StartingPrice.Should().Be(request.StartingPrice);
        result.Status.Should().Be("Draft");
        _tenderRepo.Received(1).Add(Arg.Any<Tender>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        _validator.ValidateAsync(Arg.Any<CreateTenderCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Title", "Bos olamaz") }));

        var act = () => _handler.Handle(CreateValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
