using Licit.MailService.Application.DTOs;

namespace Licit.MailService.UnitTests.Application.DTOs;

public class AuthLoginTwoFactorEmailTemplateTests
{
    [Fact]
    public void BuildSubject_ShouldReturnExpectedSubject()
    {
        AuthLoginTwoFactorEmailTemplate.BuildSubject().Should().Be("Licit giriş doğrulama kodunuz");
    }

    [Fact]
    public void BuildBody_ShouldIncludeCodeEmailAndExpiry()
    {
        var emailEvent = new AuthLoginTwoFactorEmailEvent(
            "user@example.com",
            "123456",
            new DateTime(2026, 4, 14, 18, 30),
            "Aykut"
        );

        var body = AuthLoginTwoFactorEmailTemplate.BuildBody(emailEvent);

        body.Should().Contain("123456");
        body.Should().Contain("user@example.com");
        body.Should().Contain("14.04.2026 18:30");
        body.Should().Contain("Merhaba Aykut");
    }
}
