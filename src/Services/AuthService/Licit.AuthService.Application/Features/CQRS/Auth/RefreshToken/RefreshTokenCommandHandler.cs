using FlashMediator;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.RefreshToken;

public class RefreshTokenCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    JwtSettings jwtSettings) : IRequestHandler<RefreshTokenCommandRequest, RefreshTokenCommandResponse>
{
    public async Task<RefreshTokenCommandResponse> Handle(RefreshTokenCommandRequest request, CancellationToken cancellationToken)
    {
        var userId = tokenService.ValidateRefreshToken(request.RefreshToken)
            ?? throw new UnauthorizedAccessException("Geçersiz veya süresi dolmuş refresh token.");

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

        var accessToken = await tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes);

        return new RefreshTokenCommandResponse(accessToken, refreshToken, expiresAt);
    }
}
