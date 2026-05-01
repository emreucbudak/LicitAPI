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
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher = Substitute.For<IPasswordHasher<ApplicationUser>>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IValidator<ChangePasswordCommandRequest> _validator = Substitute.For<IValidator<ChangePasswordCommandRequest>>();
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<ChangePasswordCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new ChangePasswordCommandHandler(
            _userManager,
            _passwordHistoryRepository,
            _passwordHasher,
            _currentUserService,
            _validator);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldChangePasswordAndRotateLastThreeHistoryEntries()
    {
        var userId = Guid.NewGuid();
        var oldestHistory = new PasswordHistory { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "history-hash-3" };
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "current-password-hash"
        };

        _currentUserService.UserId.Returns(userId);
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
        _userManager.ChangePasswordAsync(user, "CurrentPassword123!", "NewPassword123!").Returns(IdentityResult.Success);

        var result = await _handler.Handle(
            new ChangePasswordCommandRequest("CurrentPassword123!", "NewPassword123!"),
            CancellationToken.None);

        result.IsChanged.Should().BeTrue();
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

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _passwordHasher.VerifyHashedPassword(user, "current-password-hash", "WrongPassword123!")
            .Returns(PasswordVerificationResult.Failed);

        var act = () => _handler.Handle(
            new ChangePasswordCommandRequest("WrongPassword123!", "NewPassword123!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<CurrentPasswordInvalidException>();
        await _userManager.DidNotReceive().ChangePasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>());
        await _passwordHistoryRepository.DidNotReceive().AddAsync(Arg.Any<PasswordHistory>(), Arg.Any<CancellationToken>());
        await _passwordHistoryRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NewPasswordMatchesHistory_ShouldThrowPasswordReuseNotAllowedException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@test.com",
            PasswordHash = "current-password-hash"
        };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _passwordHasher.VerifyHashedPassword(user, "current-password-hash", "CurrentPassword123!")
            .Returns(PasswordVerificationResult.Success);
        _passwordHistoryRepository.GetLatestByUserIdAsync(userId, 3, Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new PasswordHistory { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "history-hash-1" },
                new PasswordHistory { Id = Guid.NewGuid(), UserId = userId, PasswordHash = "history-hash-2" }
            });
        _passwordHasher.VerifyHashedPassword(Arg.Any<ApplicationUser>(), "history-hash-1", "NewPassword123!")
            .Returns(PasswordVerificationResult.Success);

        var act = () => _handler.Handle(
            new ChangePasswordCommandRequest("CurrentPassword123!", "NewPassword123!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<PasswordReuseNotAllowedException>();
        await _userManager.DidNotReceive().ChangePasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>());
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
            PasswordHash = "current-password-hash"
        };

        _currentUserService.UserId.Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _passwordHasher.VerifyHashedPassword(user, "current-password-hash", "CurrentPassword123!")
            .Returns(PasswordVerificationResult.Success);
        _passwordHistoryRepository.GetLatestByUserIdAsync(userId, 3, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<PasswordHistory>());
        _userManager.ChangePasswordAsync(user, "CurrentPassword123!", "NewPassword123!")
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Password validation failed" }));

        var act = () => _handler.Handle(
            new ChangePasswordCommandRequest("CurrentPassword123!", "NewPassword123!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Password validation failed*");
        await _passwordHistoryRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
