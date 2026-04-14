using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.Login.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyLogin;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyLogin.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Licit.AuthService.UnitTests.Common;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Licit.AuthService.UnitTests.Application.Handlers;

public class VerifyLoginCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager = UserManagerMockHelper.CreateMock();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly ILoginVerificationCodeStore _loginVerificationCodeStore = Substitute.For<ILoginVerificationCodeStore>();
    private readonly JwtSettings _jwtSettings = new() { Secret = "test", Issuer = "test", Audience = "test", AccessTokenExpirationMinutes = 15 };
    private readonly IValidator<VerifyLoginCommandRequest> _validator = Substitute.For<IValidator<VerifyLoginCommandRequest>>();
    private readonly VerifyLoginCommandHandler _handler;

    public VerifyLoginCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<VerifyLoginCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new VerifyLoginCommandHandler(
            _userManager,
            _tokenService,
            _loginVerificationCodeStore,
            _jwtSettings,
            _validator);
    }

    [Fact]
    public async Task Handle_ValidCode_ShouldReturnTokensAndRemoveStoredChallenge()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Email = "test@test.com", FirstName = "Ali", LastName = "Veli", IsActive = true };
        var request = new VerifyLoginCommandRequest("test@test.com", "123456", userId, "test@test.com", "challenge-1");

        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _loginVerificationCodeStore.GetAsync("test@test.com", Arg.Any<CancellationToken>())
            .Returns(new LoginVerificationChallenge("123456", "challenge-1"));
        _tokenService.GenerateAccessTokenAsync(user).Returns("access-token");
        _tokenService.GenerateRefreshToken(userId).Returns("refresh-token");

        var result = await _handler.Handle(request, CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        await _loginVerificationCodeStore.Received(1).RemoveAsync("test@test.com", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailDoesNotMatchTemporaryToken_ShouldThrowInvalidVerificationCode()
    {
        var request = new VerifyLoginCommandRequest("test@test.com", "123456", Guid.NewGuid(), "other@test.com", "challenge-1");

        var act = () => _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidVerificationCodeException>();
    }

    [Fact]
    public async Task Handle_WrongCode_ShouldThrowInvalidVerificationCode()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Email = "test@test.com", IsActive = true };
        var request = new VerifyLoginCommandRequest("test@test.com", "123456", userId, "test@test.com", "challenge-1");

        _userManager.FindByIdAsync(userId.ToString()).Returns(user);
        _loginVerificationCodeStore.GetAsync("test@test.com", Arg.Any<CancellationToken>())
            .Returns(new LoginVerificationChallenge("654321", "challenge-1"));

        var act = () => _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidVerificationCodeException>();
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldThrowAccountDisabled()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Email = "test@test.com", IsActive = false };
        var request = new VerifyLoginCommandRequest("test@test.com", "123456", userId, "test@test.com", "challenge-1");

        _userManager.FindByIdAsync(userId.ToString()).Returns(user);

        var act = () => _handler.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<AccountDisabledException>();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        _validator.ValidateAsync(Arg.Any<VerifyLoginCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Code", "Gecersiz kod") }));

        var act = () => _handler.Handle(
            new VerifyLoginCommandRequest("test@test.com", "1", Guid.NewGuid(), "test@test.com", "challenge-1"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
