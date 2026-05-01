using FlashMediator;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Queries.GetProfile;

public class GetProfileQueryHandler(
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUserService) : IRequestHandler<GetProfileQueryRequest, GetProfileQueryResponse>
{
    public async Task<GetProfileQueryResponse> Handle(GetProfileQueryRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("Gecersiz kullanici oturumu.");

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedException("Gecersiz kullanici oturumu.");

        var roles = await userManager.GetRolesAsync(user);

        return new GetProfileQueryResponse(
            Id: user.Id.ToString(),
            Email: user.Email,
            Role: roles.FirstOrDefault(),
            FirstName: user.FirstName,
            LastName: user.LastName);
    }
}
