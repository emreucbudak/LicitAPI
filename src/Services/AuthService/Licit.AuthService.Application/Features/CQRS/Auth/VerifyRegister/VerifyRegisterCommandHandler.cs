using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.Common;
using Licit.AuthService.Application.Constants;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.Register.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyRegister;

public class VerifyRegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IRegisterVerificationStore registerVerificationStore,
    JwtSettings jwtSettings,
    IValidator<VerifyRegisterCommandRequest> validator) : IRequestHandler<VerifyRegisterCommandRequest, VerifyRegisterCommandResponse>
{
    public async Task<VerifyRegisterCommandResponse> Handle(VerifyRegisterCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tokenPayload = tokenService.ValidateTemporaryToken(request.TemporaryToken, AuthTokenTypes.PendingRegister);
        if (tokenPayload is null)
            throw new UnauthorizedException("Gecersiz veya suresi dolmus kayit dogrulama tokeni.");

        var pendingRegistration = await registerVerificationStore.GetAsync(request.TemporaryToken, cancellationToken);
        if (pendingRegistration is null || pendingRegistration.ExpiresAtUtc <= DateTime.UtcNow)
        {
            await registerVerificationStore.RemoveAsync(request.TemporaryToken, cancellationToken);
            throw new UnauthorizedException("Gecersiz veya suresi dolmus kayit dogrulama istegi.");
        }

        if (!string.Equals(tokenPayload.Email, pendingRegistration.Email, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(tokenPayload.TokenId, pendingRegistration.ChallengeId, StringComparison.Ordinal))
        {
            await registerVerificationStore.RemoveAsync(request.TemporaryToken, cancellationToken);
            throw new UnauthorizedException("Kayit dogrulama oturumu gecersiz.");
        }

        if (!VerificationCodeHelper.CodesMatch(pendingRegistration.Code, request.Code))
        {
            await HandleFailedAttemptAsync(request.TemporaryToken, pendingRegistration, cancellationToken);
            throw new UnauthorizedException("Dogrulama kodu gecersiz.");
        }

        var existingUser = await userManager.FindByEmailAsync(pendingRegistration.Email);
        if (existingUser != null)
        {
            await registerVerificationStore.RemoveAsync(request.TemporaryToken, cancellationToken);
            throw new EmailAlreadyExistsException();
        }

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = pendingRegistration.Email,
            Email = pendingRegistration.Email,
            FirstName = pendingRegistration.FirstName,
            LastName = pendingRegistration.LastName,
            PasswordHash = pendingRegistration.PasswordHash
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            throw new UserCreationFailedException(string.Join(", ", createResult.Errors.Select(e => e.Description)));

        var roleResult = await userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
            throw new UserCreationFailedException(string.Join(", ", roleResult.Errors.Select(e => e.Description)));

        await registerVerificationStore.RemoveAsync(request.TemporaryToken, cancellationToken);

        var accessToken = await tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes);

        return new VerifyRegisterCommandResponse(accessToken, refreshToken, expiresAt);
    }

    private async Task HandleFailedAttemptAsync(
        string temporaryToken,
        PendingRegistrationVerification pendingRegistration,
        CancellationToken cancellationToken)
    {
        var remainingAttempts = pendingRegistration.RemainingAttempts - 1;
        if (remainingAttempts <= 0)
        {
            await registerVerificationStore.RemoveAsync(temporaryToken, cancellationToken);
            return;
        }

        var remainingLifetime = pendingRegistration.ExpiresAtUtc - DateTime.UtcNow;
        if (remainingLifetime <= TimeSpan.Zero)
        {
            await registerVerificationStore.RemoveAsync(temporaryToken, cancellationToken);
            return;
        }

        pendingRegistration.RemainingAttempts = remainingAttempts;
        await registerVerificationStore.StoreAsync(temporaryToken, pendingRegistration, remainingLifetime, cancellationToken);
    }
}
