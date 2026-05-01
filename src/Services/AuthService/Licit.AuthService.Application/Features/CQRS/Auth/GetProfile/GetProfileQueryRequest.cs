using FlashMediator;
using Licit.AuthService.Application.DTOs;

namespace Licit.AuthService.Application.Features.CQRS.Auth.GetProfile;

public record GetProfileQueryRequest : IRequest<UserProfileDto>;
