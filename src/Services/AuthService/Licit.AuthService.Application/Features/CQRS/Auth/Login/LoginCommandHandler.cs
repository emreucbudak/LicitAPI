using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.Login.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Login;

public class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    ILoginVerificationCodeStore loginVerificationCodeStore,
    ILoginEmailPublisher loginEmailPublisher,
    TwoFactorLoginSettings twoFactorLoginSettings,
    IValidator<LoginCommandRequest> validator) : IRequestHandler<LoginCommandRequest, LoginCommandResponse>
{
    public async Task<LoginCommandResponse> Handle(LoginCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
            throw new InvalidCredentialsException();

        if (!user.IsActive)
            throw new AccountDisabledException();

        var email = user.Email ?? request.Email;
        var verificationCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var challengeId = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddMinutes(twoFactorLoginSettings.VerificationCodeExpirationMinutes);
        var lifetime = expiresAt - DateTime.UtcNow;
        var temporaryToken = tokenService.GenerateTemporaryLoginToken(user, expiresAt, challengeId);
        var userName = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();

        await loginVerificationCodeStore.StoreAsync(
            email,
            new LoginVerificationChallenge(verificationCode, challengeId),
            lifetime,
            cancellationToken);

        try
        {
            await loginEmailPublisher.PublishLoginVerificationCodeAsync(
                email,
                verificationCode,
                expiresAt,
                string.IsNullOrWhiteSpace(userName) ? null : userName,
                cancellationToken);
        }
        catch
        {
            await loginVerificationCodeStore.RemoveAsync(email, cancellationToken);
            throw;
        }

        return new LoginCommandResponse(temporaryToken, expiresAt);
    }
}
