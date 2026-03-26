using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.Register.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Register;

public class RegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    JwtSettings jwtSettings,
    IValidator<RegisterCommandRequest> validator) : IRequestHandler<RegisterCommandRequest, RegisterCommandResponse>
{
    public async Task<RegisterCommandResponse> Handle(RegisterCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new EmailAlreadyExistsException();

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new UserCreationFailedException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, "User");

        var accessToken = await tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes);

        return new RegisterCommandResponse(accessToken, refreshToken, expiresAt);
    }
}
