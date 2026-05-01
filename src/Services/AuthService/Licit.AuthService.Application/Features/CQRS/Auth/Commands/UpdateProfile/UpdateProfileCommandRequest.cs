using FlashMediator;
using Licit.AuthService.Application.DTOs;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.UpdateProfile;

public record UpdateProfileCommandRequest(
    string FirstName,
    string LastName
) : IRequest<UserProfileDto>;
