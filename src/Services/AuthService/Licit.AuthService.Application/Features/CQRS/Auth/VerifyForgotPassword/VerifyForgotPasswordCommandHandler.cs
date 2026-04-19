using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.Common;
using Licit.AuthService.Application.Constants;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Interfaces;

namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyForgotPassword;

public class VerifyForgotPasswordCommandHandler(
    ITokenService tokenService,
    IPasswordResetVerificationStore passwordResetVerificationStore,
    IValidator<VerifyForgotPasswordCommandRequest> validator) : IRequestHandler<VerifyForgotPasswordCommandRequest, VerifyForgotPasswordCommandResponse>
{
    public async Task<VerifyForgotPasswordCommandResponse> Handle(VerifyForgotPasswordCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var tokenPayload = tokenService.ValidateTemporaryToken(request.TemporaryToken, AuthTokenTypes.PendingPasswordReset);
        if (tokenPayload is null)
            throw new UnauthorizedException("Gecersiz veya suresi dolmus sifre sifirlama tokeni.");

        var challenge = await passwordResetVerificationStore.GetAsync(request.TemporaryToken, cancellationToken);
        if (challenge is null || challenge.ExpiresAtUtc <= DateTime.UtcNow)
        {
            await passwordResetVerificationStore.RemoveAsync(request.TemporaryToken, cancellationToken);
            throw new UnauthorizedException("Gecersiz veya suresi dolmus sifre sifirlama istegi.");
        }

        if (!string.Equals(tokenPayload.Email, challenge.Email, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(tokenPayload.TokenId, challenge.ChallengeId, StringComparison.Ordinal))
        {
            await passwordResetVerificationStore.RemoveAsync(request.TemporaryToken, cancellationToken);
            throw new UnauthorizedException("Sifre sifirlama oturumu gecersiz.");
        }

        if (!VerificationCodeHelper.CodesMatch(challenge.Code, request.Code))
        {
            await HandleFailedAttemptAsync(request.TemporaryToken, challenge, cancellationToken);
            throw new UnauthorizedException("Dogrulama kodu gecersiz.");
        }

        var remainingLifetime = challenge.ExpiresAtUtc - DateTime.UtcNow;
        if (remainingLifetime <= TimeSpan.Zero)
        {
            await passwordResetVerificationStore.RemoveAsync(request.TemporaryToken, cancellationToken);
            throw new UnauthorizedException("Gecersiz veya suresi dolmus sifre sifirlama istegi.");
        }

        challenge.IsCodeVerified = true;
        challenge.Code = string.Empty;
        await passwordResetVerificationStore.StoreAsync(request.TemporaryToken, challenge, remainingLifetime, cancellationToken);

        return new VerifyForgotPasswordCommandResponse(true);
    }

    private async Task HandleFailedAttemptAsync(
        string temporaryToken,
        PasswordResetVerificationChallenge challenge,
        CancellationToken cancellationToken)
    {
        var remainingAttempts = challenge.RemainingAttempts - 1;
        if (remainingAttempts <= 0)
        {
            await passwordResetVerificationStore.RemoveAsync(temporaryToken, cancellationToken);
            return;
        }

        var remainingLifetime = challenge.ExpiresAtUtc - DateTime.UtcNow;
        if (remainingLifetime <= TimeSpan.Zero)
        {
            await passwordResetVerificationStore.RemoveAsync(temporaryToken, cancellationToken);
            return;
        }

        challenge.RemainingAttempts = remainingAttempts;
        await passwordResetVerificationStore.StoreAsync(temporaryToken, challenge, remainingLifetime, cancellationToken);
    }
}
