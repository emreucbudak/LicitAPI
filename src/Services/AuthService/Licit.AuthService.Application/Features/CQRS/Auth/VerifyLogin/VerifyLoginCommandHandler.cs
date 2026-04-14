using System.Security.Cryptography;
using System.Text;
using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.Login.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyLogin.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyLogin;

public class VerifyLoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    ILoginVerificationCodeStore loginVerificationCodeStore,
    JwtSettings jwtSettings,
    IValidator<VerifyLoginCommandRequest> validator) : IRequestHandler<VerifyLoginCommandRequest, VerifyLoginCommandResponse>
{
    public async Task<VerifyLoginCommandResponse> Handle(VerifyLoginCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (!string.Equals(request.Email, request.TemporaryTokenEmail, StringComparison.OrdinalIgnoreCase))
            throw new InvalidVerificationCodeException();

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null || !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            throw new InvalidVerificationCodeException();

        if (!user.IsActive)
            throw new AccountDisabledException();

        var storedChallenge = await loginVerificationCodeStore.GetAsync(request.Email, cancellationToken);
        if (storedChallenge == null
            || !string.Equals(storedChallenge.ChallengeId, request.TemporaryTokenId, StringComparison.Ordinal)
            || !CodesMatch(storedChallenge.Code, request.Code))
            throw new InvalidVerificationCodeException();

        await loginVerificationCodeStore.RemoveAsync(request.Email, cancellationToken);

        var accessToken = await tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes);

        return new VerifyLoginCommandResponse(accessToken, refreshToken, expiresAt);
    }

    private static bool CodesMatch(string expectedCode, string actualCode)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expectedCode);
        var actualBytes = Encoding.UTF8.GetBytes(actualCode);

        return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}
