using FlashMediator;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.GetProfile;

public class GetProfileQueryHandler(
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUserService) : IRequestHandler<GetProfileQueryRequest, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(GetProfileQueryRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("Gecersiz kullanici oturumu.");

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedException("Gecersiz kullanici oturumu.");

        var roles = await userManager.GetRolesAsync(user);

        return new UserProfileDto(
            Id: user.Id.ToString(),
            Email: user.Email,
            Role: roles.FirstOrDefault(),
            FirstName: user.FirstName,
            LastName: user.LastName);
    }
}
