using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.Commands.ChangePassword;
using Licit.AuthService.Application.Features.CQRS.Auth.Commands.ChangePassword.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Licit.AuthService.UnitTests.Common;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Licit.AuthService.UnitTests.Application.Handlers;

public class ChangePasswordCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager = UserManagerMockHelper.CreateMock();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly IValidator<ChangePasswordCommandRequest> _validator = Substitute.For<IValidator<ChangePasswordCommandRequest>>();
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<ChangePasswordCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new ChangePasswordCommandHandler(
            _userManager,
            _currentUserService,
            _validator);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldChangePassword()
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
        _userManager.CheckPasswordAsync(user, "CurrentPassword123!").Returns(true);
        _userManager.ChangePasswordAsync(user, "CurrentPassword123!", "NewPassword123!").Returns(IdentityResult.Success);

        var result = await _handler.Handle(
            new ChangePasswordCommandRequest("CurrentPassword123!", "NewPassword123!"),
            CancellationToken.None);

        result.IsChanged.Should().BeTrue();
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
        _userManager.CheckPasswordAsync(user, "WrongPassword123!").Returns(false);

        var act = () => _handler.Handle(
            new ChangePasswordCommandRequest("WrongPassword123!", "NewPassword123!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<CurrentPasswordInvalidException>();
        await _userManager.DidNotReceive().ChangePasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>());
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
        _userManager.CheckPasswordAsync(user, "CurrentPassword123!").Returns(true);
        _userManager.ChangePasswordAsync(user, "CurrentPassword123!", "NewPassword123!")
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Password validation failed" }));

        var act = () => _handler.Handle(
            new ChangePasswordCommandRequest("CurrentPassword123!", "NewPassword123!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Password validation failed*");
    }
}
