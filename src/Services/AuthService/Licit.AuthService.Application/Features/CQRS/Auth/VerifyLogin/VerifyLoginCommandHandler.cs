using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.Common;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Exceptions;
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
    ICurrentUserService currentUserService,
    IValidator<VerifyLoginCommandRequest> validator) : IRequestHandler<VerifyLoginCommandRequest, VerifyLoginCommandResponse>
{
    public async Task<VerifyLoginCommandResponse> Handle(VerifyLoginCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("Gecersiz gecici oturum.");
        var temporaryTokenEmail = currentUserService.Email;
        var temporaryTokenId = currentUserService.TokenId;

        if (string.IsNullOrWhiteSpace(temporaryTokenEmail) || string.IsNullOrWhiteSpace(temporaryTokenId))
            throw new UnauthorizedException("Gecersiz gecici oturum.");

        if (!string.Equals(request.Email, temporaryTokenEmail, StringComparison.OrdinalIgnoreCase))
            throw new InvalidVerificationCodeException();

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || !string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            throw new InvalidVerificationCodeException();

        if (!user.IsActive)
            throw new AccountDisabledException();

        var storedChallenge = await loginVerificationCodeStore.GetAsync(request.Email, cancellationToken);
        if (storedChallenge == null
            || !string.Equals(storedChallenge.ChallengeId, temporaryTokenId, StringComparison.Ordinal)
            || !VerificationCodeHelper.CodesMatch(storedChallenge.Code, request.Code))
            throw new InvalidVerificationCodeException();

        await loginVerificationCodeStore.RemoveAsync(request.Email, cancellationToken);

        var accessToken = await tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes);

        return new VerifyLoginCommandResponse(accessToken, refreshToken, expiresAt);
    }
}
