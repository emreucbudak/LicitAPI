using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.Register;
using Licit.AuthService.Application.Features.CQRS.Auth.Register.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Licit.AuthService.UnitTests.Common;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Licit.AuthService.UnitTests.Application.Handlers;

public class RegisterCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager = UserManagerMockHelper.CreateMock();
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher = Substitute.For<IPasswordHasher<ApplicationUser>>();
    private readonly IPasswordFingerprintService _passwordFingerprintService = Substitute.For<IPasswordFingerprintService>();
    private readonly IRegisterVerificationStore _registerVerificationStore = Substitute.For<IRegisterVerificationStore>();
    private readonly IEmailBloomService _emailBloomService = Substitute.For<IEmailBloomService>();
    private readonly ILoginEmailPublisher _loginEmailPublisher = Substitute.For<ILoginEmailPublisher>();
    private readonly AuthVerificationSettings _authVerificationSettings = new() { RegisterVerificationCodeExpirationMinutes = 10, MaxVerificationAttempts = 5 };
    private readonly IValidator<RegisterCommandRequest> _validator = Substitute.For<IValidator<RegisterCommandRequest>>();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<RegisterCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new RegisterCommandHandler(
            _userManager,
            _passwordHasher,
            _passwordFingerprintService,
            _registerVerificationStore,
            _emailBloomService,
            _loginEmailPublisher,
            _authVerificationSettings,
            _validator);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldStorePendingRegistrationByEmailPublishEmailAndReturnChallengeDetails()
    {
        PendingRegistrationVerification? storedVerification = null;

        _emailBloomService.MayExistAsync("test@test.com", Arg.Any<CancellationToken>()).Returns(false);
        _passwordHasher.HashPassword(Arg.Any<ApplicationUser>(), "Password123!").Returns("hashed-password");
        _passwordFingerprintService.CreateFingerprint("Password123!").Returns("password-fingerprint");
        _registerVerificationStore
            .When(x => x.StoreAsync(
                "test@test.com",
                Arg.Any<PendingRegistrationVerification>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo => storedVerification = callInfo.ArgAt<PendingRegistrationVerification>(1));

        var result = await _handler.Handle(
            new RegisterCommandRequest(" test@test.com ", "Password123!", " Ali ", " Veli "),
            CancellationToken.None);

        result.Email.Should().Be("test@test.com");
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        storedVerification.Should().NotBeNull();
        storedVerification!.Email.Should().Be("test@test.com");
        storedVerification.FirstName.Should().Be("Ali");
        storedVerification.LastName.Should().Be("Veli");
        storedVerification.PasswordHash.Should().Be("hashed-password");
        storedVerification.PasswordFingerprint.Should().Be("password-fingerprint");
        storedVerification.RemainingAttempts.Should().Be(5);
        storedVerification.Code.Should().MatchRegex(@"^\d{6}$");
        await _loginEmailPublisher.Received(1).PublishRegisterVerificationCodeAsync(
            "test@test.com",
            storedVerification.Code,
            Arg.Any<DateTime>(),
            "Ali Veli",
            Arg.Any<CancellationToken>());
        await _emailBloomService.Received(1).MayExistAsync("test@test.com", Arg.Any<CancellationToken>());
        await _userManager.DidNotReceive().FindByEmailAsync(Arg.Any<string>());
        await _userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>());
        await _userManager.DidNotReceive().AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_EmailBloomMaybeAndUserExists_ShouldThrow()
    {
        _emailBloomService.MayExistAsync("test@test.com", Arg.Any<CancellationToken>()).Returns(true);
        var existingUser = new ApplicationUser { Email = "test@test.com" };
        _userManager.FindByEmailAsync("test@test.com").Returns(existingUser);

        var act = () => _handler.Handle(
            new RegisterCommandRequest("test@test.com", "Password123!", "Ali", "Veli"),
            CancellationToken.None);

        await act.Should().ThrowAsync<EmailAlreadyExistsException>();
    }

    [Fact]
    public async Task Handle_EmailPublishFails_ShouldRemoveStoredVerificationAndRethrow()
    {
        _emailBloomService.MayExistAsync("test@test.com", Arg.Any<CancellationToken>()).Returns(false);
        _passwordHasher.HashPassword(Arg.Any<ApplicationUser>(), "Password123!").Returns("hashed-password");
        _passwordFingerprintService.CreateFingerprint("Password123!").Returns("password-fingerprint");
        _loginEmailPublisher
            .PublishRegisterVerificationCodeAsync(
                "test@test.com",
                Arg.Any<string>(),
                Arg.Any<DateTime>(),
                "Ali Veli",
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("RabbitMQ unavailable")));

        var act = () => _handler.Handle(
            new RegisterCommandRequest("test@test.com", "Password123!", "Ali", "Veli"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _registerVerificationStore.Received(1).RemoveAsync("test@test.com", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        _validator.ValidateAsync(Arg.Any<RegisterCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Email", "Gecersiz email") }));

        var act = () => _handler.Handle(
            new RegisterCommandRequest(string.Empty, "Password123!", "Ali", "Veli"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_EmailBloomMaybeButNoExactMatch_ShouldContinue()
    {
        _emailBloomService.MayExistAsync("test@test.com", Arg.Any<CancellationToken>()).Returns(true);
        _userManager.FindByEmailAsync("test@test.com").Returns((ApplicationUser?)null);
        _passwordHasher.HashPassword(Arg.Any<ApplicationUser>(), "Password123!").Returns("hashed-password");
        _passwordFingerprintService.CreateFingerprint("Password123!").Returns("password-fingerprint");

        var result = await _handler.Handle(
            new RegisterCommandRequest("test@test.com", "Password123!", "Ali", "Veli"),
            CancellationToken.None);

        result.Email.Should().Be("test@test.com");
        await _userManager.Received(1).FindByEmailAsync("test@test.com");
        await _registerVerificationStore.Received(1).StoreAsync(
            "test@test.com",
            Arg.Any<PendingRegistrationVerification>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
    }
}
