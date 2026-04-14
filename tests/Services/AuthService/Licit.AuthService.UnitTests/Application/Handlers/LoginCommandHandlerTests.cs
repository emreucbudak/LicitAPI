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
    private readonly ILoginVerificationCodeStore _loginVerificationCodeStore = Substitute.For<ILoginVerificationCodeStore>();
    private readonly ILoginEmailPublisher _loginEmailPublisher = Substitute.For<ILoginEmailPublisher>();
    private readonly TwoFactorLoginSettings _twoFactorLoginSettings = new() { VerificationCodeExpirationMinutes = 5 };
    private readonly IValidator<LoginCommandRequest> _validator = Substitute.For<IValidator<LoginCommandRequest>>();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<LoginCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new LoginCommandHandler(
            _userManager,
            _tokenService,
            _loginVerificationCodeStore,
            _loginEmailPublisher,
            _twoFactorLoginSettings,
            _validator);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldStoreCodePublishEmailAndReturnTemporaryToken()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@test.com", FirstName = "Ali", LastName = "Veli", IsActive = true };
        string? generatedCode = null;

        _userManager.FindByEmailAsync("test@test.com").Returns(user);
        _userManager.CheckPasswordAsync(user, "Password123!").Returns(true);
        _tokenService.GenerateTemporaryLoginToken(user, Arg.Any<DateTime>(), Arg.Any<string>()).Returns("temporary-token");
        _loginVerificationCodeStore
            .When(x => x.StoreAsync(
                Arg.Any<string>(),
                Arg.Any<LoginVerificationChallenge>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo => generatedCode = callInfo.ArgAt<LoginVerificationChallenge>(1).Code);

        var result = await _handler.Handle(new LoginCommandRequest("test@test.com", "Password123!"), CancellationToken.None);

        result.TemporaryToken.Should().Be("temporary-token");
        generatedCode.Should().NotBeNull();
        generatedCode.Should().HaveLength(6);
        generatedCode.Should().MatchRegex(@"^\d{6}$");
        await _loginVerificationCodeStore.Received(1).StoreAsync(
            "test@test.com",
            Arg.Is<LoginVerificationChallenge>(challenge => challenge.Code == generatedCode && !string.IsNullOrWhiteSpace(challenge.ChallengeId)),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
        await _loginEmailPublisher.Received(1).PublishLoginVerificationCodeAsync(
            "test@test.com",
            generatedCode!,
            Arg.Any<DateTime>(),
            "Ali Veli",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailPublishFails_ShouldRemoveStoredCodeAndRethrow()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@test.com", FirstName = "Ali", LastName = "Veli", IsActive = true };

        _userManager.FindByEmailAsync("test@test.com").Returns(user);
        _userManager.CheckPasswordAsync(user, "Password123!").Returns(true);
        _tokenService.GenerateTemporaryLoginToken(user, Arg.Any<DateTime>(), Arg.Any<string>()).Returns("temporary-token");
        _loginEmailPublisher
            .PublishLoginVerificationCodeAsync(
                "test@test.com",
                Arg.Any<string>(),
                Arg.Any<DateTime>(),
                "Ali Veli",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("RabbitMQ unavailable")));

        var act = () => _handler.Handle(new LoginCommandRequest("test@test.com", "Password123!"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _loginVerificationCodeStore.Received(1).RemoveAsync("test@test.com", Arg.Any<CancellationToken>());
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
