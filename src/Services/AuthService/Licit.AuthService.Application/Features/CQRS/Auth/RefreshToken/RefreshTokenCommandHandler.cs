using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.RefreshToken.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.RefreshToken;

public class RefreshTokenCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    JwtSettings jwtSettings,
    IValidator<RefreshTokenCommandRequest> validator) : IRequestHandler<RefreshTokenCommandRequest, RefreshTokenCommandResponse>
{
    public async Task<RefreshTokenCommandResponse> Handle(RefreshTokenCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var userId = tokenService.ValidateRefreshToken(request.RefreshToken)
            ?? throw new InvalidRefreshTokenException();

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new UserFromTokenNotFoundException(userId);

        var accessToken = await tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes);

        return new RefreshTokenCommandResponse(accessToken, refreshToken, expiresAt);
    }
}
