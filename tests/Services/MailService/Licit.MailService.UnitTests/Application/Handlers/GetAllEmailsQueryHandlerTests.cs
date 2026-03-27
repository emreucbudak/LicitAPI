using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.MailService.Application.Features.CQRS.Email.GetAll;
using Licit.MailService.Application.Interfaces;
using Licit.MailService.Domain.Entities;
using NSubstitute;

namespace Licit.MailService.UnitTests.Application.Handlers;

public class GetAllEmailsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IEmailRepository _emailRepo = Substitute.For<IEmailRepository>();
    private readonly IValidator<GetAllEmailsQueryRequest> _validator = Substitute.For<IValidator<GetAllEmailsQueryRequest>>();
    private readonly GetAllEmailsQueryHandler _handler;

    public GetAllEmailsQueryHandlerTests()
    {
        _unitOfWork.Emails.Returns(_emailRepo);
        _validator.ValidateAsync(Arg.Any<GetAllEmailsQueryRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new GetAllEmailsQueryHandler(_unitOfWork, _validator);
    }

    [Fact]
    public async Task Handle_WithEmails_ShouldReturnMappedList()
    {
        var email1 = new EmailMessage("a@a.com", "Konu 1", "Gövde 1");
        var email2 = new EmailMessage("b@b.com", "Konu 2", "Gövde 2");
        _emailRepo.GetAllAsync(1, 20).Returns(new List<EmailMessage> { email1, email2 });

        var result = await _handler.Handle(new GetAllEmailsQueryRequest(1, 20), CancellationToken.None);

        result.Emails.Should().HaveCount(2);
        result.Emails[0].To.Should().Be("a@a.com");
    }

    [Fact]
    public async Task Handle_NoEmails_ShouldReturnEmptyList()
    {
        _emailRepo.GetAllAsync(1, 20).Returns(Enumerable.Empty<EmailMessage>());

        var result = await _handler.Handle(new GetAllEmailsQueryRequest(1, 20), CancellationToken.None);

        result.Emails.Should().BeEmpty();
    }
}
