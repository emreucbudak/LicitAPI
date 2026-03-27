using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.MailService.Application.Features.CQRS.Email.GetById;
using Licit.MailService.Application.Features.CQRS.Email.GetById.Exceptions;
using Licit.MailService.Application.Interfaces;
using Licit.MailService.Domain.Entities;
using NSubstitute;

namespace Licit.MailService.UnitTests.Application.Handlers;

public class GetEmailByIdQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IEmailRepository _emailRepo = Substitute.For<IEmailRepository>();
    private readonly IValidator<GetEmailByIdQueryRequest> _validator = Substitute.For<IValidator<GetEmailByIdQueryRequest>>();
    private readonly GetEmailByIdQueryHandler _handler;

    public GetEmailByIdQueryHandlerTests()
    {
        _unitOfWork.Emails.Returns(_emailRepo);
        _validator.ValidateAsync(Arg.Any<GetEmailByIdQueryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new GetEmailByIdQueryHandler(_unitOfWork, _validator);
    }

    [Fact]
    public async Task Handle_EmailExists_ShouldReturnResponse()
    {
        var email = new EmailMessage("test@test.com", "Konu", "Gövde");
        _emailRepo.GetByIdAsync(email.Id).Returns(email);

        var result = await _handler.Handle(new GetEmailByIdQueryRequest(email.Id), CancellationToken.None);

        result.Id.Should().Be(email.Id);
        result.To.Should().Be("test@test.com");
        result.Body.Should().Be("Gövde");
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_EmailNotFound_ShouldThrow()
    {
        var id = Guid.NewGuid();
        _emailRepo.GetByIdAsync(id).Returns((EmailMessage?)null);

        var act = () => _handler.Handle(new GetEmailByIdQueryRequest(id), CancellationToken.None);

        await act.Should().ThrowAsync<EmailNotFoundException>();
    }
}
