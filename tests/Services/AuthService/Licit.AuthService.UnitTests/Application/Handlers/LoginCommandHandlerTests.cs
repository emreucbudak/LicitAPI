using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.Login;
using Licit.AuthService.Application.Features.CQRS.Auth.Login.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Licit.AuthService.UnitTests.Common;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Licit.AuthService.UnitTests.Application.Handlers;

public class LoginCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager = UserManagerMockHelper.CreateMock();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly JwtSettings _jwtSettings = new() { Secret = "test", Issuer = "test", Audience = "test", AccessTokenExpirationMinutes = 15 };
    private readonly IValidator<LoginCommandRequest> _validator = Substitute.For<IValidator<LoginCommandRequest>>();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<LoginCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new LoginCommandHandler(_userManager, _tokenService, _jwtSettings, _validator);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnTokens()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@test.com", FirstName = "Ali", LastName = "Veli", IsActive = true };
        _userManager.FindByEmailAsync("test@test.com").Returns(user);
        _userManager.CheckPasswordAsync(user, "Password123!").Returns(true);
        _tokenService.GenerateAccessTokenAsync(user).Returns("access-token");
        _tokenService.GenerateRefreshToken(user.Id).Returns("refresh-token");

        var result = await _handler.Handle(new LoginCommandRequest("test@test.com", "Password123!"), CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowInvalidCredentials()
    {
        _userManager.FindByEmailAsync("test@test.com").Returns((ApplicationUser?)null);

        var act = () => _handler.Handle(new LoginCommandRequest("test@test.com", "Password123!"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldThrowInvalidCredentials()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@test.com", IsActive = true };
        _userManager.FindByEmailAsync("test@test.com").Returns(user);
        _userManager.CheckPasswordAsync(user, "wrong").Returns(false);

        var act = () => _handler.Handle(new LoginCommandRequest("test@test.com", "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldThrowAccountDisabled()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@test.com", IsActive = false };
        _userManager.FindByEmailAsync("test@test.com").Returns(user);
        _userManager.CheckPasswordAsync(user, "Password123!").Returns(true);

        var act = () => _handler.Handle(new LoginCommandRequest("test@test.com", "Password123!"), CancellationToken.None);

        await act.Should().ThrowAsync<AccountDisabledException>();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        _validator.ValidateAsync(Arg.Any<LoginCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Email", "Boş olamaz") }));

        var act = () => _handler.Handle(new LoginCommandRequest("", ""), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
