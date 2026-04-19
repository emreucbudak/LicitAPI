using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.Common;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.ForgotPassword;

public class ForgotPasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IPasswordResetVerificationStore passwordResetVerificationStore,
    ILoginEmailPublisher loginEmailPublisher,
    AuthVerificationSettings authVerificationSettings,
    IValidator<ForgotPasswordCommandRequest> validator) : IRequestHandler<ForgotPasswordCommandRequest, ForgotPasswordCommandResponse>
{
    public async Task<ForgotPasswordCommandResponse> Handle(ForgotPasswordCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var email = request.Email.Trim();
        var challengeId = Guid.NewGuid().ToString("N");
        var verificationCode = VerificationCodeHelper.GenerateSixDigitCode();
        var expiresAt = DateTime.UtcNow.AddMinutes(authVerificationSettings.PasswordResetCodeExpirationMinutes);
        var lifetime = expiresAt - DateTime.UtcNow;
        var temporaryToken = tokenService.GenerateTemporaryPasswordResetToken(email, expiresAt, challengeId);
        var user = await userManager.FindByEmailAsync(email);

        await passwordResetVerificationStore.StoreAsync(
            temporaryToken,
            new PasswordResetVerificationChallenge
            {
                UserId = user?.Id,
                Email = email,
                Code = verificationCode,
                ChallengeId = challengeId,
                ExpiresAtUtc = expiresAt,
                RemainingAttempts = authVerificationSettings.MaxVerificationAttempts,
                IsCodeVerified = false
            },
            lifetime,
            cancellationToken);

        if (user is not null)
        {
            var userName = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();

            try
            {
                await loginEmailPublisher.PublishPasswordResetCodeAsync(
                    email,
                    verificationCode,
                    expiresAt,
                    string.IsNullOrWhiteSpace(userName) ? null : userName,
                    cancellationToken);
            }
            catch
            {
                await passwordResetVerificationStore.RemoveAsync(temporaryToken, cancellationToken);
                throw;
            }
        }

        return new ForgotPasswordCommandResponse(temporaryToken, expiresAt, email);
    }
}
