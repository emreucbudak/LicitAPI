using FlashMediator;
using Licit.AuthService.Application.DTOs;

namespace Licit.AuthService.Application.Features.CQRS.Auth.UpdateProfile;

public record UpdateProfileCommandRequest(
    string FirstName,
    string LastName
) : IRequest<UserProfileDto>;
