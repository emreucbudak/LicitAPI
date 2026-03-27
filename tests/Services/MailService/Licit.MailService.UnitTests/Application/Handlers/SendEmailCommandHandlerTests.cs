using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.MailService.Application.Features.CQRS.Email.Send;
using Licit.MailService.Application.Features.CQRS.Email.Send.Exceptions;
using Licit.MailService.Application.Interfaces;
using Licit.MailService.Domain.Entities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Licit.MailService.UnitTests.Application.Handlers;

public class SendEmailCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IEmailRepository _emailRepo = Substitute.For<IEmailRepository>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IValidator<SendEmailCommandRequest> _validator = Substitute.For<IValidator<SendEmailCommandRequest>>();
    private readonly SendEmailCommandHandler _handler;

    public SendEmailCommandHandlerTests()
    {
        _unitOfWork.Emails.Returns(_emailRepo);
        _validator.ValidateAsync(Arg.Any<SendEmailCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new SendEmailCommandHandler(_unitOfWork, _emailSender, _validator);
    }

    [Fact]
    public async Task Handle_SuccessfulSend_ShouldReturnSentStatus()
    {
        var request = new SendEmailCommandRequest("test@test.com", "Konu", "Gövde");

        var result = await _handler.Handle(request, CancellationToken.None);

        result.To.Should().Be("test@test.com");
        result.Subject.Should().Be("Konu");
        result.Status.Should().Be("Sent");
        result.SentAt.Should().NotBeNull();
        _emailRepo.Received(1).Add(Arg.Any<EmailMessage>());
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SendFails_ShouldThrowEmailSendFailedException()
    {
        _emailSender.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("SMTP hatası"));

        var act = () => _handler.Handle(new SendEmailCommandRequest("test@test.com", "Konu", "Gövde"), CancellationToken.None);

        await act.Should().ThrowAsync<EmailSendFailedException>();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        _validator.ValidateAsync(Arg.Any<SendEmailCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("To", "Boş olamaz") }));

        var act = () => _handler.Handle(new SendEmailCommandRequest("", "Konu", "Gövde"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
