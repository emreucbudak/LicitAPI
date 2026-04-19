using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.ForgotPassword;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Licit.AuthService.UnitTests.Common;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Licit.AuthService.UnitTests.Application.Handlers;

public class ForgotPasswordCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager = UserManagerMockHelper.CreateMock();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IPasswordResetVerificationStore _passwordResetVerificationStore = Substitute.For<IPasswordResetVerificationStore>();
    private readonly ILoginEmailPublisher _loginEmailPublisher = Substitute.For<ILoginEmailPublisher>();
    private readonly AuthVerificationSettings _authVerificationSettings = new() { PasswordResetCodeExpirationMinutes = 10, MaxVerificationAttempts = 5 };
    private readonly IValidator<ForgotPasswordCommandRequest> _validator = Substitute.For<IValidator<ForgotPasswordCommandRequest>>();
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<ForgotPasswordCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new ForgotPasswordCommandHandler(
            _userManager,
            _tokenService,
            _passwordResetVerificationStore,
            _loginEmailPublisher,
            _authVerificationSettings,
            _validator);
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldStoreChallengePublishEmailAndReturnTemporaryToken()
    {
        PasswordResetVerificationChallenge? storedChallenge = null;
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@test.com",
            FirstName = "Ali",
            LastName = "Veli"
        };

        _userManager.FindByEmailAsync("test@test.com").Returns(user);
        _tokenService.GenerateTemporaryPasswordResetToken("test@test.com", Arg.Any<DateTime>(), Arg.Any<string>())
            .Returns("temporary-token");
        _passwordResetVerificationStore
            .When(x => x.StoreAsync(
                "temporary-token",
                Arg.Any<PasswordResetVerificationChallenge>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo => storedChallenge = callInfo.ArgAt<PasswordResetVerificationChallenge>(1));

        var result = await _handler.Handle(new ForgotPasswordCommandRequest(" test@test.com "), CancellationToken.None);

        result.TemporaryToken.Should().Be("temporary-token");
        result.Email.Should().Be("test@test.com");
        storedChallenge.Should().NotBeNull();
        storedChallenge!.UserId.Should().Be(userId);
        storedChallenge.IsCodeVerified.Should().BeFalse();
        storedChallenge.RemainingAttempts.Should().Be(5);
        storedChallenge.Code.Should().MatchRegex(@"^\d{6}$");
        await _loginEmailPublisher.Received(1).PublishPasswordResetCodeAsync(
            "test@test.com",
            storedChallenge.Code,
            Arg.Any<DateTime>(),
            "Ali Veli",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserDoesNotExist_ShouldStillStoreChallengeWithoutPublishingEmail()
    {
        PasswordResetVerificationChallenge? storedChallenge = null;

        _userManager.FindByEmailAsync("missing@test.com").Returns((ApplicationUser?)null);
        _tokenService.GenerateTemporaryPasswordResetToken("missing@test.com", Arg.Any<DateTime>(), Arg.Any<string>())
            .Returns("temporary-token");
        _passwordResetVerificationStore
            .When(x => x.StoreAsync(
                "temporary-token",
                Arg.Any<PasswordResetVerificationChallenge>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo => storedChallenge = callInfo.ArgAt<PasswordResetVerificationChallenge>(1));

        var result = await _handler.Handle(new ForgotPasswordCommandRequest("missing@test.com"), CancellationToken.None);

        result.TemporaryToken.Should().Be("temporary-token");
        storedChallenge.Should().NotBeNull();
        storedChallenge!.UserId.Should().BeNull();
        await _loginEmailPublisher.DidNotReceive().PublishPasswordResetCodeAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailPublishFails_ShouldRemoveStoredChallengeAndRethrow()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            FirstName = "Ali",
            LastName = "Veli"
        };

        _userManager.FindByEmailAsync("test@test.com").Returns(user);
        _tokenService.GenerateTemporaryPasswordResetToken("test@test.com", Arg.Any<DateTime>(), Arg.Any<string>())
            .Returns("temporary-token");
        _loginEmailPublisher
            .PublishPasswordResetCodeAsync(
                "test@test.com",
                Arg.Any<string>(),
                Arg.Any<DateTime>(),
                "Ali Veli",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("RabbitMQ unavailable")));

        var act = () => _handler.Handle(new ForgotPasswordCommandRequest("test@test.com"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _passwordResetVerificationStore.Received(1).RemoveAsync("temporary-token", Arg.Any<CancellationToken>());
    }
}
