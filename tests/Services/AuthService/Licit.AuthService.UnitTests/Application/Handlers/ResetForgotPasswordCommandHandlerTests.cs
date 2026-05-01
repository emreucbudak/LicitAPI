using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.AuthService.Application.Constants;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.ChangePassword.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.ResetForgotPassword;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Licit.AuthService.UnitTests.Common;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Licit.AuthService.UnitTests.Application.Handlers;

public class ResetForgotPasswordCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager = UserManagerMockHelper.CreateMock();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IPasswordResetVerificationStore _passwordResetVerificationStore = Substitute.For<IPasswordResetVerificationStore>();
    private readonly IPasswordHistoryRepository _passwordHistoryRepository = Substitute.For<IPasswordHistoryRepository>();
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher = Substitute.For<IPasswordHasher<ApplicationUser>>();
    private readonly IValidator<ResetForgotPasswordCommandRequest> _validator = Substitute.For<IValidator<ResetForgotPasswordCommandRequest>>();
    private readonly ResetForgotPasswordCommandHandler _handler;

    public ResetForgotPasswordCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<ResetForgotPasswordCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new ResetForgotPasswordCommandHandler(
            _userManager,
            _tokenService,
            _passwordResetVerificationStore,
            _passwordHistoryRepository,
            _passwordHasher,
            _validator);
    }

    [Fact]
    public async Task Handle_VerifiedChallenge_ShouldRotateHistoryAndRemoveChallenge()
    {
        var userId = Guid.NewGuid();
        var temporaryToken = "temporary-token";
        var oldestHistory = new PasswordHistory { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "history-hash-3" };
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "current-password-hash"
        };
        var challenge = new PasswordResetVerificationChallenge
        {
            UserId = userId,
            Email = "test@test.com",
            Code = string.Empty,
            ChallengeId = "challenge-1",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            RemainingAttempts = 4,
            IsCodeVerified = true
        };

        _tokenService.ValidateTemporaryToken(temporaryToken, AuthTokenTypes.PendingPasswordReset)
            .Returns(new TemporaryTokenPayload("test@test.com", "challenge-1", AuthTokenTypes.PendingPasswordReset));
        _passwordResetVerificationStore.GetAsync(temporaryToken, Arg.Any<CancellationToken>()).Returns(challenge);
        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _passwordHistoryRepository.GetLatestByUserIdAsync(userId, 3, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new PasswordHistory { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "history-hash-1" },
                new PasswordHistory { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "history-hash-2" },
                oldestHistory
            });
        _userManager.GeneratePasswordResetTokenAsync(user).Returns("identity-reset-token");
        _userManager.ResetPasswordAsync(user, "identity-reset-token", "NewPassword123!").Returns(IdentityResult.Success);

        var result = await _handler.Handle(
            new ResetForgotPasswordCommandRequest(temporaryToken, "NewPassword123!"),
            CancellationToken.None);

        result.IsReset.Should().BeTrue();
        await _passwordHistoryRepository.Received(1).AddAsync(
            Arg.Is<PasswordHistory>(history =>
                history.UserId == userId
                && history.PasswordHash == "current-password-hash"),
            Arg.Any<CancellationToken>());
        _passwordHistoryRepository.Received(1).RemoveRange(Arg.Is<IEnumerable<PasswordHistory>>(histories =>
            histories.Count() == 1
            && histories.Single().Id == oldestHistory.Id));
        await _userManager.Received(1).GeneratePasswordResetTokenAsync(user);
        await _userManager.Received(1).ResetPasswordAsync(user, "identity-reset-token", "NewPassword123!");
        await _passwordHistoryRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _passwordResetVerificationStore.Received(1).RemoveAsync(temporaryToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CodeNotVerified_ShouldThrowBusinessRuleException()
    {
        var temporaryToken = "temporary-token";
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

        var act = () => _handler.Handle(
            new ResetForgotPasswordCommandRequest(temporaryToken, "NewPassword123!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        await _passwordResetVerificationStore.DidNotReceive().RemoveAsync(temporaryToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NewPasswordMatchesHistory_ShouldThrowPasswordReuseNotAllowedException()
    {
        var userId = Guid.NewGuid();
        var temporaryToken = "temporary-token";
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "current-password-hash"
        };
        var challenge = new PasswordResetVerificationChallenge
        {
            UserId = userId,
            Email = "test@test.com",
            Code = string.Empty,
            ChallengeId = "challenge-1",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            RemainingAttempts = 4,
            IsCodeVerified = true
        };

        _tokenService.ValidateTemporaryToken(temporaryToken, AuthTokenTypes.PendingPasswordReset)
            .Returns(new TemporaryTokenPayload("test@test.com", "challenge-1", AuthTokenTypes.PendingPasswordReset));
        _passwordResetVerificationStore.GetAsync(temporaryToken, Arg.Any<CancellationToken>()).Returns(challenge);
        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _passwordHistoryRepository.GetLatestByUserIdAsync(userId, 3, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new PasswordHistory { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "history-hash-1" },
                new PasswordHistory { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "history-hash-2" }
            });
        _passwordHasher.VerifyHashedPassword(Arg.Any<ApplicationUser>(), "history-hash-2", "NewPassword123!")
            .Returns(PasswordVerificationResult.Success);

        var act = () => _handler.Handle(
            new ResetForgotPasswordCommandRequest(temporaryToken, "NewPassword123!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<PasswordReuseNotAllowedException>();
        await _userManager.DidNotReceive().GeneratePasswordResetTokenAsync(Arg.Any<ApplicationUser>());
        await _passwordHistoryRepository.DidNotReceive().AddAsync(Arg.Any<PasswordHistory>(), Arg.Any<CancellationToken>());
        await _passwordHistoryRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _passwordResetVerificationStore.DidNotReceive().RemoveAsync(temporaryToken, Arg.Any<CancellationToken>());
    }
}
