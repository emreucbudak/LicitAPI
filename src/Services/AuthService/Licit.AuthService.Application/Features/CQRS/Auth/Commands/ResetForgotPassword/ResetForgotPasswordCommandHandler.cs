using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.Common;
using Licit.AuthService.Application.Constants;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.Commands.ChangePassword.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.ResetForgotPassword;

public class ResetForgotPasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IPasswordResetVerificationStore passwordResetVerificationStore,
    IPasswordHistoryRepository passwordHistoryRepository,
    IPasswordHasher<ApplicationUser> passwordHasher,
    IValidator<ResetForgotPasswordCommandRequest> validator) : IRequestHandler<ResetForgotPasswordCommandRequest, ResetForgotPasswordCommandResponse>
{
    private const int PasswordHistoryLimit = 3;

    public async Task<ResetForgotPasswordCommandResponse> Handle(ResetForgotPasswordCommandRequest request, CancellationToken cancellationToken)
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

        if (!challenge.IsCodeVerified)
            throw new BusinessRuleException("Sifre sifirlama kodu once dogrulanmalidir.");

        if (!string.Equals(tokenPayload.Email, challenge.Email, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(tokenPayload.TokenId, challenge.ChallengeId, StringComparison.Ordinal))
        {
            await passwordResetVerificationStore.RemoveAsync(request.TemporaryToken, cancellationToken);
            throw new UnauthorizedException("Sifre sifirlama oturumu gecersiz.");
        }

        if (challenge.UserId is Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is not null)
            {
                var latestPasswordHistory = await passwordHistoryRepository.GetLatestByUserIdAsync(
                    user.Id,
                    PasswordHistoryLimit,
                    cancellationToken);

                if (PasswordReuseHelper.MatchesCurrentOrHistory(user, request.NewPassword, latestPasswordHistory, passwordHasher))
                    throw new PasswordReuseNotAllowedException();

                var currentPasswordHash = user.PasswordHash;
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
                if (!resetResult.Succeeded)
                    throw new BusinessRuleException(string.Join(", ", resetResult.Errors.Select(e => e.Description)));

                await passwordHistoryRepository.AddPreviousPasswordAsync(
                    user.Id,
                    currentPasswordHash,
                    PasswordHistoryLimit,
                    cancellationToken);
            }
        }

        await passwordResetVerificationStore.RemoveAsync(request.TemporaryToken, cancellationToken);
        return new ResetForgotPasswordCommandResponse(true);
    }
}
