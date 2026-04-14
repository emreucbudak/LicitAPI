using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyLogin;

public record VerifyLoginCommandRequest(
    string Email,
    string Code,
    Guid UserId,
    string TemporaryTokenEmail,
    string TemporaryTokenId
) : IRequest<VerifyLoginCommandResponse>;
