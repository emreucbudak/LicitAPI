using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.UpdateProfile;

public class UpdateProfileCommandHandler(
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUserService,
    IValidator<UpdateProfileCommandRequest> validator) : IRequestHandler<UpdateProfileCommandRequest, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateProfileCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("Gecersiz kullanici oturumu.");

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedException("Gecersiz kullanici oturumu.");

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new BusinessRuleException(string.Join(", ", updateResult.Errors.Select(error => error.Description)));

        var roles = await userManager.GetRolesAsync(user);

        return new UserProfileDto(
            Id: user.Id.ToString(),
            Email: user.Email,
            Role: roles.FirstOrDefault(),
            FirstName: user.FirstName,
            LastName: user.LastName);
    }
}
