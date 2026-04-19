using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.AuthService.Application.Constants;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.Register.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyRegister;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Licit.AuthService.UnitTests.Common;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Licit.AuthService.UnitTests.Application.Handlers;

public class VerifyRegisterCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager = UserManagerMockHelper.CreateMock();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IRegisterVerificationStore _registerVerificationStore = Substitute.For<IRegisterVerificationStore>();
    private readonly JwtSettings _jwtSettings = new() { Secret = "test", Issuer = "test", Audience = "test", AccessTokenExpirationMinutes = 15 };
    private readonly IValidator<VerifyRegisterCommandRequest> _validator = Substitute.For<IValidator<VerifyRegisterCommandRequest>>();
    private readonly VerifyRegisterCommandHandler _handler;

    public VerifyRegisterCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<VerifyRegisterCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new VerifyRegisterCommandHandler(
            _userManager,
            _tokenService,
            _registerVerificationStore,
            _jwtSettings,
            _validator);
    }

    [Fact]
    public async Task Handle_ValidCode_ShouldCreateUserReturnTokensAndRemoveChallenge()
    {
        var temporaryToken = "temporary-token";
        var request = new VerifyRegisterCommandRequest(temporaryToken, "123456");
        var pendingRegistration = new PendingRegistrationVerification
        {
            Email = "test@test.com",
            FirstName = "Ali",
            LastName = "Veli",
            PasswordHash = "hashed-password",
            Code = "123456",
            ChallengeId = "challenge-1",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            RemainingAttempts = 5
        };

        _tokenService.ValidateTemporaryToken(temporaryToken, AuthTokenTypes.PendingRegister)
            .Returns(new TemporaryTokenPayload("test@test.com", "challenge-1", AuthTokenTypes.PendingRegister));
        _registerVerificationStore.GetAsync(temporaryToken, Arg.Any<CancellationToken>()).Returns(pendingRegistration);
        _userManager.FindByEmailAsync("test@test.com").Returns((ApplicationUser?)null);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), "User").Returns(IdentityResult.Success);
        _tokenService.GenerateAccessTokenAsync(Arg.Any<ApplicationUser>()).Returns("access-token");
        _tokenService.GenerateRefreshToken(Arg.Any<Guid>()).Returns("refresh-token");

        var result = await _handler.Handle(request, CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        await _userManager.Received(1).CreateAsync(Arg.Is<ApplicationUser>(user =>
            user.Email == "test@test.com"
            && user.UserName == "test@test.com"
            && user.FirstName == "Ali"
            && user.LastName == "Veli"
            && user.PasswordHash == "hashed-password"));
        await _userManager.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), "User");
        await _registerVerificationStore.Received(1).RemoveAsync(temporaryToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WrongCode_ShouldDecrementAttemptsAndThrowUnauthorized()
    {
        var temporaryToken = "temporary-token";
        PendingRegistrationVerification? updatedVerification = null;
        var pendingRegistration = new PendingRegistrationVerification
        {
            Email = "test@test.com",
            FirstName = "Ali",
            LastName = "Veli",
            PasswordHash = "hashed-password",
            Code = "654321",
            ChallengeId = "challenge-1",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            RemainingAttempts = 5
        };

        _tokenService.ValidateTemporaryToken(temporaryToken, AuthTokenTypes.PendingRegister)
            .Returns(new TemporaryTokenPayload("test@test.com", "challenge-1", AuthTokenTypes.PendingRegister));
        _registerVerificationStore.GetAsync(temporaryToken, Arg.Any<CancellationToken>()).Returns(pendingRegistration);
        _registerVerificationStore
            .When(x => x.StoreAsync(
                temporaryToken,
                Arg.Any<PendingRegistrationVerification>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo => updatedVerification = callInfo.ArgAt<PendingRegistrationVerification>(1));

        var act = () => _handler.Handle(new VerifyRegisterCommandRequest(temporaryToken, "123456"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
        updatedVerification.Should().NotBeNull();
        updatedVerification!.RemainingAttempts.Should().Be(4);
        await _registerVerificationStore.DidNotReceive().RemoveAsync(temporaryToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ShouldRemoveChallengeAndThrow()
    {
        var temporaryToken = "temporary-token";
        var pendingRegistration = new PendingRegistrationVerification
        {
            Email = "test@test.com",
            FirstName = "Ali",
            LastName = "Veli",
            PasswordHash = "hashed-password",
            Code = "123456",
            ChallengeId = "challenge-1",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            RemainingAttempts = 5
        };

        _tokenService.ValidateTemporaryToken(temporaryToken, AuthTokenTypes.PendingRegister)
            .Returns(new TemporaryTokenPayload("test@test.com", "challenge-1", AuthTokenTypes.PendingRegister));
        _registerVerificationStore.GetAsync(temporaryToken, Arg.Any<CancellationToken>()).Returns(pendingRegistration);
        _userManager.FindByEmailAsync("test@test.com").Returns(new ApplicationUser { Email = "test@test.com" });

        var act = () => _handler.Handle(new VerifyRegisterCommandRequest(temporaryToken, "123456"), CancellationToken.None);

        await act.Should().ThrowAsync<EmailAlreadyExistsException>();
        await _registerVerificationStore.Received(1).RemoveAsync(temporaryToken, Arg.Any<CancellationToken>());
    }
}
