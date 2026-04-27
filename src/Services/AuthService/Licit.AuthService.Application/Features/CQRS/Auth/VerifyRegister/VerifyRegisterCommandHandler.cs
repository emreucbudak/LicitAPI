using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.Common;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.Register.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyRegister;

public class VerifyRegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
    IRegisterVerificationStore registerVerificationStore,
    IEmailBloomService emailBloomService,
    IUserPasswordBloomService userPasswordBloomService,
    IValidator<VerifyRegisterCommandRequest> validator) : IRequestHandler<VerifyRegisterCommandRequest, VerifyRegisterCommandResponse>
{
    public async Task<VerifyRegisterCommandResponse> Handle(VerifyRegisterCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var email = request.Email.Trim();

        var pendingRegistration = await registerVerificationStore.GetAsync(email, cancellationToken);
        if (pendingRegistration is null || pendingRegistration.ExpiresAtUtc <= DateTime.UtcNow)
        {
            await registerVerificationStore.RemoveAsync(email, cancellationToken);
            throw new UnauthorizedException("Gecersiz veya suresi dolmus kayit dogrulama istegi.");
        }

        if (!string.Equals(email, pendingRegistration.Email, StringComparison.OrdinalIgnoreCase))
        {
            await registerVerificationStore.RemoveAsync(email, cancellationToken);
            throw new UnauthorizedException("Kayit dogrulama oturumu gecersiz.");
        }

        if (!VerificationCodeHelper.CodesMatch(pendingRegistration.Code, request.Code))
        {
            await HandleFailedAttemptAsync(email, pendingRegistration, cancellationToken);
            throw new UnauthorizedException("Dogrulama kodu gecersiz.");
        }

        var existingUser = await userManager.FindByEmailAsync(pendingRegistration.Email);
        if (existingUser != null)
        {
            await registerVerificationStore.RemoveAsync(email, cancellationToken);
            throw new EmailAlreadyExistsException();
        }

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = pendingRegistration.Email,
            Email = pendingRegistration.Email,
            FirstName = pendingRegistration.FirstName,
            LastName = pendingRegistration.LastName,
            PasswordHash = pendingRegistration.PasswordHash,
            CurrentPasswordFingerprint = pendingRegistration.PasswordFingerprint
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            throw new UserCreationFailedException(string.Join(", ", createResult.Errors.Select(e => e.Description)));

        var roleResult = await userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
            throw new UserCreationFailedException(string.Join(", ", roleResult.Errors.Select(e => e.Description)));

        await emailBloomService.AddAsync(pendingRegistration.Email, cancellationToken);
        await userPasswordBloomService.SetFingerprintsAsync(
            user.Id,
            new[] { pendingRegistration.PasswordFingerprint },
            cancellationToken);

        await registerVerificationStore.RemoveAsync(email, cancellationToken);

        return new VerifyRegisterCommandResponse(true);
    }

    private async Task HandleFailedAttemptAsync(
        string email,
        PendingRegistrationVerification pendingRegistration,
        CancellationToken cancellationToken)
    {
        var remainingAttempts = pendingRegistration.RemainingAttempts - 1;
        if (remainingAttempts <= 0)
        {
            await registerVerificationStore.RemoveAsync(email, cancellationToken);
            return;
        }

        var remainingLifetime = pendingRegistration.ExpiresAtUtc - DateTime.UtcNow;
        if (remainingLifetime <= TimeSpan.Zero)
        {
            await registerVerificationStore.RemoveAsync(email, cancellationToken);
            return;
        }

        pendingRegistration.RemainingAttempts = remainingAttempts;
        await registerVerificationStore.StoreAsync(email, pendingRegistration, remainingLifetime, cancellationToken);
    }
}
