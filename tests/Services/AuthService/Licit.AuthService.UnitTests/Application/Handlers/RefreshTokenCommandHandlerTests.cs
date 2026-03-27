using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.RefreshToken;
using Licit.AuthService.Application.Features.CQRS.Auth.RefreshToken.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Licit.AuthService.UnitTests.Common;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Licit.AuthService.UnitTests.Application.Handlers;

public class RefreshTokenCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager = UserManagerMockHelper.CreateMock();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly JwtSettings _jwtSettings = new() { Secret = "test", Issuer = "test", Audience = "test", AccessTokenExpirationMinutes = 15 };
    private readonly IValidator<RefreshTokenCommandRequest> _validator = Substitute.For<IValidator<RefreshTokenCommandRequest>>();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<RefreshTokenCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new RefreshTokenCommandHandler(_userManager, _tokenService, _jwtSettings, _validator);
    }

    [Fact]
    public async Task Handle_ValidToken_ShouldReturnNewTokens()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Email = "test@test.com" };
        _tokenService.ValidateRefreshToken("valid-token").Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _tokenService.GenerateAccessTokenAsync(user).Returns("new-access");
        _tokenService.GenerateRefreshToken(userId).Returns("new-refresh");

        var result = await _handler.Handle(new RefreshTokenCommandRequest("valid-token"), CancellationToken.None);

        result.AccessToken.Should().Be("new-access");
        result.RefreshToken.Should().Be("new-refresh");
    }

    [Fact]
    public async Task Handle_InvalidToken_ShouldThrowInvalidRefreshToken()
    {
        _tokenService.ValidateRefreshToken("bad-token").Returns((Guid?)null);

        var act = () => _handler.Handle(new RefreshTokenCommandRequest("bad-token"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldThrowUserFromTokenNotFoundException()
    {
        var userId = Guid.NewGuid();
        _tokenService.ValidateRefreshToken("valid-token").Returns(userId);
        _userManager.FindByIdAsync(userId.ToString()).Returns((ApplicationUser?)null);

        var act = () => _handler.Handle(new RefreshTokenCommandRequest("valid-token"), CancellationToken.None);

        await act.Should().ThrowAsync<UserFromTokenNotFoundException>();
    }
}
