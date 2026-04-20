using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.ChangePassword;
using Licit.AuthService.Application.Features.CQRS.Auth.ChangePassword.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Licit.AuthService.UnitTests.Common;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Licit.AuthService.UnitTests.Application.Handlers;

public class ChangePasswordCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager = UserManagerMockHelper.CreateMock();
    private readonly IPasswordHistoryRepository _passwordHistoryRepository = Substitute.For<IPasswordHistoryRepository>();
    private readonly IUserPasswordBloomService _userPasswordBloomService = Substitute.For<IUserPasswordBloomService>();
    private readonly IPasswordFingerprintService _passwordFingerprintService = Substitute.For<IPasswordFingerprintService>();
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher = Substitute.For<IPasswordHasher<ApplicationUser>>();
    private readonly IValidator<ChangePasswordCommandRequest> _validator = Substitute.For<IValidator<ChangePasswordCommandRequest>>();
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<ChangePasswordCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new ChangePasswordCommandHandler(
            _userManager,
            _passwordHistoryRepository,
            _userPasswordBloomService,
            _passwordFingerprintService,
            _passwordHasher,
            _validator);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldChangePasswordRotateHistoryAndRefreshFingerprints()
    {
        var userId = Guid.NewGuid();
        var oldestHistory = new PasswordHistory { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "history-hash-3" };
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "current-password-hash",
            CurrentPasswordFingerprint = "current-fingerprint"
        };

        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _passwordHasher.VerifyHashedPassword(user, "current-password-hash", "CurrentPassword123!")
            .Returns(PasswordVerificationResult.Success);
        _passwordHistoryRepository.GetLatestByUserIdAsync(userId, 3, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new PasswordHistory { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "history-hash-1" },
                new PasswordHistory { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "history-hash-2" },
                oldestHistory
            });
        _passwordFingerprintService.CreateFingerprint("NewPassword123!").Returns("new-fingerprint");
        _userPasswordBloomService.MayContainAsync(userId, "new-fingerprint", Arg.Any<CancellationToken>()).Returns(false);
        _userManager.ChangePasswordAsync(user, "CurrentPassword123!", "NewPassword123!").Returns(IdentityResult.Success);
        _userPasswordBloomService.GetFingerprintsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new[] { "current-fingerprint", "older-fingerprint-1", "older-fingerprint-2", "older-fingerprint-3" });

        var result = await _handler.Handle(
            new ChangePasswordCommandRequest(userId, "CurrentPassword123!", "NewPassword123!"),
            CancellationToken.None);

        result.IsChanged.Should().BeTrue();
        user.CurrentPasswordFingerprint.Should().Be("new-fingerprint");
        await _passwordHistoryRepository.Received(1).AddAsync(
            Arg.Is<PasswordHistory>(history =>
                history.UserId == userId
                && history.PasswordHash == "current-password-hash"),
            Arg.Any<CancellationToken>());
        _passwordHistoryRepository.Received(1).RemoveRange(Arg.Is<IEnumerable<PasswordHistory>>(histories =>
            histories.Count() == 1
            && histories.Single().Id == oldestHistory.Id));
        await _passwordHistoryRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _userManager.Received(1).ChangePasswordAsync(user, "CurrentPassword123!", "NewPassword123!");
        await _userPasswordBloomService.Received(1).SetFingerprintsAsync(
            userId,
            Arg.Is<IReadOnlyCollection<string>>(fingerprints =>
                fingerprints.SequenceEqual(new[]
                {
                    "new-fingerprint",
                    "current-fingerprint",
                    "older-fingerprint-1",
                    "older-fingerprint-2"
                })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CurrentPasswordMismatch_ShouldThrowCurrentPasswordInvalidException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "current-password-hash"
        };

        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _passwordHasher.VerifyHashedPassword(user, "current-password-hash", "WrongPassword123!")
            .Returns(PasswordVerificationResult.Failed);

        var act = () => _handler.Handle(
            new ChangePasswordCommandRequest(userId, "WrongPassword123!", "NewPassword123!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<CurrentPasswordInvalidException>();
        await _userManager.DidNotReceive().ChangePasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>());
        await _passwordHistoryRepository.DidNotReceive().AddAsync(Arg.Any<PasswordHistory>(), Arg.Any<CancellationToken>());
        await _passwordHistoryRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BloomHitAndHistoryMatch_ShouldThrowPasswordReuseNotAllowedException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "current-password-hash",
            CurrentPasswordFingerprint = "current-fingerprint"
        };

        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _passwordHasher.VerifyHashedPassword(user, "current-password-hash", "CurrentPassword123!")
            .Returns(PasswordVerificationResult.Success);
        _passwordHistoryRepository.GetLatestByUserIdAsync(userId, 3, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new PasswordHistory { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "history-hash-1" },
                new PasswordHistory { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "history-hash-2" }
            });
        _userPasswordBloomService.GetFingerprintsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new[] { "current-fingerprint", "older-fingerprint-1", "older-fingerprint-2" });
        _passwordFingerprintService.CreateFingerprint("NewPassword123!").Returns("new-fingerprint");
        _userPasswordBloomService.MayContainAsync(userId, "new-fingerprint", Arg.Any<CancellationToken>()).Returns(true);
        _passwordHasher.VerifyHashedPassword(Arg.Any<ApplicationUser>(), Arg.Any<string>(), "NewPassword123!")
            .Returns(callInfo =>
            {
                var passwordHash = callInfo.ArgAt<string>(1);
                return passwordHash == "history-hash-1"
                    ? PasswordVerificationResult.Success
                    : PasswordVerificationResult.Failed;
            });

        var act = () => _handler.Handle(
            new ChangePasswordCommandRequest(userId, "CurrentPassword123!", "NewPassword123!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<PasswordReuseNotAllowedException>();
        await _userManager.DidNotReceive().ChangePasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>());
        await _userPasswordBloomService.DidNotReceive().SetFingerprintsAsync(
            Arg.Any<Guid>(),
            Arg.Any<IReadOnlyCollection<string>>(),
            Arg.Any<CancellationToken>());
        await _passwordHistoryRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_IdentityChangeFails_ShouldThrowBusinessRuleException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "current-password-hash",
            CurrentPasswordFingerprint = "current-fingerprint"
        };

        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _passwordHasher.VerifyHashedPassword(user, "current-password-hash", "CurrentPassword123!")
            .Returns(PasswordVerificationResult.Success);
        _passwordHistoryRepository.GetLatestByUserIdAsync(userId, 3, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<PasswordHistory>());
        _userPasswordBloomService.GetFingerprintsAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new[] { "current-fingerprint" });
        _passwordFingerprintService.CreateFingerprint("NewPassword123!").Returns("new-fingerprint");
        _userPasswordBloomService.MayContainAsync(userId, "new-fingerprint", Arg.Any<CancellationToken>()).Returns(false);
        _userManager.ChangePasswordAsync(user, "CurrentPassword123!", "NewPassword123!")
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Password validation failed" }));

        var act = () => _handler.Handle(
            new ChangePasswordCommandRequest(userId, "CurrentPassword123!", "NewPassword123!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Password validation failed*");
        await _userPasswordBloomService.DidNotReceive().SetFingerprintsAsync(
            Arg.Any<Guid>(),
            Arg.Any<IReadOnlyCollection<string>>(),
            Arg.Any<CancellationToken>());
        await _passwordHistoryRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
