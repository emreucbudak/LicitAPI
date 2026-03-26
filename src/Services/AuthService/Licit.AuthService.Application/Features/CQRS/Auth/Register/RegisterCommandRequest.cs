using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Register;

public record RegisterCommandRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName
) : IRequest<RegisterCommandResponse>;
