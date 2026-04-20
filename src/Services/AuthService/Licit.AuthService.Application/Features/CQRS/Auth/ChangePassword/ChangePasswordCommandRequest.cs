using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.ChangePassword;

public record ChangePasswordCommandRequest(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<ChangePasswordCommandResponse>;
