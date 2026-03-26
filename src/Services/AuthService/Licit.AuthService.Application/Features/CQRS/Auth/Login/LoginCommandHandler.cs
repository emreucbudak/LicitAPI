using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.Login.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Login;

public class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    JwtSettings jwtSettings,
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

        var accessToken = await tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes);

        return new LoginCommandResponse(accessToken, refreshToken, expiresAt);
    }
}
