using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
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
    private readonly IRegisterVerificationStore _registerVerificationStore = Substitute.For<IRegisterVerificationStore>();
    private readonly IEmailBloomService _emailBloomService = Substitute.For<IEmailBloomService>();
    private readonly IUserPasswordBloomService _userPasswordBloomService = Substitute.For<IUserPasswordBloomService>();
    private readonly IValidator<VerifyRegisterCommandRequest> _validator = Substitute.For<IValidator<VerifyRegisterCommandRequest>>();
    private readonly VerifyRegisterCommandHandler _handler;

    public VerifyRegisterCommandHandlerTests()
    {
        _validator.ValidateAsync(Arg.Any<VerifyRegisterCommandRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _handler = new VerifyRegisterCommandHandler(
            _userManager,
            _registerVerificationStore,
            _emailBloomService,
            _userPasswordBloomService,
            _validator);
    }

    [Fact]
    public async Task Handle_ValidCode_ShouldCreateUserReturnSuccessAndRemoveChallenge()
    {
        var email = "test@test.com";
        var request = new VerifyRegisterCommandRequest(email, "123456");
        var pendingRegistration = new PendingRegistrationVerification
        {
            Email = email,
            FirstName = "Ali",
            LastName = "Veli",
            PasswordHash = "hashed-password",
            PasswordFingerprint = "password-fingerprint",
            Code = "123456",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            RemainingAttempts = 5
        };

        _registerVerificationStore.GetAsync(email, Arg.Any<CancellationToken>()).Returns(pendingRegistration);
        _userManager.FindByEmailAsync(email).Returns((ApplicationUser?)null);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
        _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), "User").Returns(IdentityResult.Success);

        var result = await _handler.Handle(request, CancellationToken.None);

        result.IsVerified.Should().BeTrue();
        await _userManager.Received(1).CreateAsync(Arg.Is<ApplicationUser>(user =>
            user.Email == email
            && user.UserName == email
            && user.FirstName == "Ali"
            && user.LastName == "Veli"
            && user.PasswordHash == "hashed-password"
            && user.CurrentPasswordFingerprint == "password-fingerprint"));
        await _userManager.DidNotReceive().CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
        await _userManager.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), "User");
        await _emailBloomService.Received(1).AddAsync(email, Arg.Any<CancellationToken>());
        await _userPasswordBloomService.Received(1).SetFingerprintsAsync(
            Arg.Any<Guid>(),
            Arg.Is<IReadOnlyCollection<string>>(fingerprints =>
                fingerprints.Count == 1
                && fingerprints.Contains("password-fingerprint")),
            Arg.Any<CancellationToken>());
        await _registerVerificationStore.Received(1).RemoveAsync(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WrongCode_ShouldDecrementAttemptsAndThrowUnauthorized()
    {
        var email = "test@test.com";
        PendingRegistrationVerification? updatedVerification = null;
        var pendingRegistration = new PendingRegistrationVerification
        {
            Email = email,
            FirstName = "Ali",
            LastName = "Veli",
            PasswordHash = "hashed-password",
            PasswordFingerprint = "password-fingerprint",
            Code = "654321",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            RemainingAttempts = 5
        };

        _registerVerificationStore.GetAsync(email, Arg.Any<CancellationToken>()).Returns(pendingRegistration);
        _registerVerificationStore
            .When(x => x.StoreAsync(
                email,
                Arg.Any<PendingRegistrationVerification>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo => updatedVerification = callInfo.ArgAt<PendingRegistrationVerification>(1));

        var act = () => _handler.Handle(new VerifyRegisterCommandRequest(email, "123456"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
        updatedVerification.Should().NotBeNull();
        updatedVerification!.RemainingAttempts.Should().Be(4);
        await _registerVerificationStore.DidNotReceive().RemoveAsync(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WrongCodeWithNoAttemptsLeft_ShouldRemoveChallengeAndThrowUnauthorized()
    {
        var email = "test@test.com";
        var pendingRegistration = new PendingRegistrationVerification
        {
            Email = email,
            FirstName = "Ali",
            LastName = "Veli",
            PasswordHash = "hashed-password",
            PasswordFingerprint = "password-fingerprint",
            Code = "654321",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            RemainingAttempts = 1
        };

        _registerVerificationStore.GetAsync(email, Arg.Any<CancellationToken>()).Returns(pendingRegistration);

        var act = () => _handler.Handle(new VerifyRegisterCommandRequest(email, "123456"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
        await _registerVerificationStore.Received(1).RemoveAsync(email, Arg.Any<CancellationToken>());
        await _registerVerificationStore.DidNotReceive().StoreAsync(
            email,
            Arg.Any<PendingRegistrationVerification>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ShouldRemoveChallengeAndThrow()
    {
        var email = "test@test.com";
        var pendingRegistration = new PendingRegistrationVerification
        {
            Email = email,
            FirstName = "Ali",
            LastName = "Veli",
            PasswordHash = "hashed-password",
            PasswordFingerprint = "password-fingerprint",
            Code = "123456",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            RemainingAttempts = 5
        };

        _registerVerificationStore.GetAsync(email, Arg.Any<CancellationToken>()).Returns(pendingRegistration);
        _userManager.FindByEmailAsync(email).Returns(new ApplicationUser { Email = email });

        var act = () => _handler.Handle(new VerifyRegisterCommandRequest(email, "123456"), CancellationToken.None);

        await act.Should().ThrowAsync<EmailAlreadyExistsException>();
        await _emailBloomService.DidNotReceive().AddAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _userPasswordBloomService.DidNotReceive().SetFingerprintsAsync(
            Arg.Any<Guid>(),
            Arg.Any<IReadOnlyCollection<string>>(),
            Arg.Any<CancellationToken>());
        await _registerVerificationStore.Received(1).RemoveAsync(email, Arg.Any<CancellationToken>());
    }
}
