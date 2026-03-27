using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.TenderingService.Application.Features.CQRS.Tender.Create;
using Licit.TenderingService.Application.Interfaces;
using Licit.TenderingService.Domain.Entities;
using NSubstitute;

namespace Licit.TenderingService.UnitTests.Application.Handlers;

public class CreateTenderCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ITenderRepository _tenderRepo = Substitute.For<ITenderRepository>();
    private readonly IValidator<CreateTenderCommandRequest> _validator = Substitute.For<IValidator<CreateTenderCommandRequest>>();
    private readonly CreateTenderCommandHandler _handler;

    public CreateTenderCommandHandlerTests()
    {
        _unitOfWork.Tenders.Returns(_tenderRepo);
        _validator.ValidateAsync(Arg.Any<CreateTenderCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new CreateTenderCommandHandler(_unitOfWork, _validator);
    }

    private CreateTenderCommandRequest CreateValidRequest() => new(
        Title: "Test İhale",
        Description: "Test açıklama",
        StartingPrice: 1000m,
        StartDate: DateTime.UtcNow.AddDays(1),
        EndDate: DateTime.UtcNow.AddDays(30),
        CreatedByUserId: Guid.NewGuid(),
        CategoryId: Guid.NewGuid(),
        Rules: null
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
    public async Task Handle_WithRules_ShouldAddRulesToTender()
    {
        var request = CreateValidRequest() with
        {
            Rules = new List<CreateTenderRuleDto>
            {
                new("Kural 1", "Açıklama 1", true),
                new("Kural 2", "Açıklama 2", false)
            }
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _tenderRepo.Received(1).Add(Arg.Is<Tender>(t => t.Rules.Count == 2));
    }

    [Fact]
    public async Task Handle_WithEmptyRules_ShouldNotAddRules()
    {
        var request = CreateValidRequest() with { Rules = new List<CreateTenderRuleDto>() };

        var result = await _handler.Handle(request, CancellationToken.None);

        result.Should().NotBeNull();
        _tenderRepo.Received(1).Add(Arg.Is<Tender>(t => t.Rules.Count == 0));
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        _validator.ValidateAsync(Arg.Any<CreateTenderCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Title", "Boş olamaz") }));

        var act = () => _handler.Handle(CreateValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
