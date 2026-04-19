using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.AuthService.Application.Constants;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyForgotPassword;
using Licit.AuthService.Application.Interfaces;
using NSubstitute;

namespace Licit.AuthService.UnitTests.Application.Handlers;

public class VerifyForgotPasswordCommandHandlerTests
{
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IPasswordResetVerificationStore _passwordResetVerificationStore = Substitute.For<IPasswordResetVerificationStore>();
    private readonly IValidator<VerifyForgotPasswordCommandRequest> _validator = Substitute.For<IValidator<VerifyForgotPasswordCommandRequest>>();
    private readonly VerifyForgotPasswordCommandHandler _handler;

    public VerifyForgotPasswordCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<VerifyForgotPasswordCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new VerifyForgotPasswordCommandHandler(
            _tokenService,
            _passwordResetVerificationStore,
            _validator);
    }

    [Fact]
    public async Task Handle_ValidCode_ShouldMarkChallengeAsVerified()
    {
        var temporaryToken = "temporary-token";
        PasswordResetVerificationChallenge? updatedChallenge = null;
        var challenge = new PasswordResetVerificationChallenge
        {
            UserId = Guid.NewGuid(),
            Email = "test@test.com",
            Code = "123456",
            ChallengeId = "challenge-1",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            RemainingAttempts = 5,
            IsCodeVerified = false
        };

        _tokenService.ValidateTemporaryToken(temporaryToken, AuthTokenTypes.PendingPasswordReset)
            .Returns(new TemporaryTokenPayload("test@test.com", "challenge-1", AuthTokenTypes.PendingPasswordReset));
        _passwordResetVerificationStore.GetAsync(temporaryToken, Arg.Any<CancellationToken>()).Returns(challenge);
        _passwordResetVerificationStore
            .When(x => x.StoreAsync(
                temporaryToken,
                Arg.Any<PasswordResetVerificationChallenge>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo => updatedChallenge = callInfo.ArgAt<PasswordResetVerificationChallenge>(1));

        var result = await _handler.Handle(new VerifyForgotPasswordCommandRequest(temporaryToken, "123456"), CancellationToken.None);

        result.IsVerified.Should().BeTrue();
        updatedChallenge.Should().NotBeNull();
        updatedChallenge!.IsCodeVerified.Should().BeTrue();
        updatedChallenge.Code.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WrongCode_ShouldDecrementAttemptsAndThrowUnauthorized()
    {
        var temporaryToken = "temporary-token";
        PasswordResetVerificationChallenge? updatedChallenge = null;
        var challenge = new PasswordResetVerificationChallenge
        {
            UserId = Guid.NewGuid(),
            Email = "test@test.com",
            Code = "654321",
            ChallengeId = "challenge-1",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            RemainingAttempts = 5,
            IsCodeVerified = false
        };

        _tokenService.ValidateTemporaryToken(temporaryToken, AuthTokenTypes.PendingPasswordReset)
            .Returns(new TemporaryTokenPayload("test@test.com", "challenge-1", AuthTokenTypes.PendingPasswordReset));
        _passwordResetVerificationStore.GetAsync(temporaryToken, Arg.Any<CancellationToken>()).Returns(challenge);
        _passwordResetVerificationStore
            .When(x => x.StoreAsync(
                temporaryToken,
                Arg.Any<PasswordResetVerificationChallenge>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo => updatedChallenge = callInfo.ArgAt<PasswordResetVerificationChallenge>(1));

        var act = () => _handler.Handle(new VerifyForgotPasswordCommandRequest(temporaryToken, "123456"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
        updatedChallenge.Should().NotBeNull();
        updatedChallenge!.RemainingAttempts.Should().Be(4);
        updatedChallenge.IsCodeVerified.Should().BeFalse();
    }
}
