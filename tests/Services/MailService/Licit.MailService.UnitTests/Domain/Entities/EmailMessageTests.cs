using FluentAssertions;
using Licit.MailService.Domain.Entities;

namespace Licit.MailService.UnitTests.Domain.Entities;

public class EmailMessageTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var email = new EmailMessage("test@test.com", "Konu", "Gövde");

        email.To.Should().Be("test@test.com");
        email.Subject.Should().Be("Konu");
        email.Body.Should().Be("Gövde");
        email.Status.Should().Be(EmailStatus.Pending);
        email.SentAt.Should().BeNull();
        email.ErrorMessage.Should().BeNull();
        email.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void MarkAsSent_ShouldUpdateStatusAndSentAt()
    {
        var email = new EmailMessage("test@test.com", "Konu", "Gövde");

        email.MarkAsSent();

        email.Status.Should().Be(EmailStatus.Sent);
        email.SentAt.Should().NotBeNull();
        email.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsFailed_ShouldUpdateStatusAndErrorMessage()
    {
        var email = new EmailMessage("test@test.com", "Konu", "Gövde");

        email.MarkAsFailed("SMTP hatası");

        email.Status.Should().Be(EmailStatus.Failed);
        email.ErrorMessage.Should().Be("SMTP hatası");
        email.UpdatedAt.Should().NotBeNull();
        email.SentAt.Should().BeNull();
    }
}
