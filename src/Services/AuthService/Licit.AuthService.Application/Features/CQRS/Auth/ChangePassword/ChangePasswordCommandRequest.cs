using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.ChangePassword;

public record ChangePasswordCommandRequest(
    string CurrentPassword,
    string NewPassword
) : IRequest<ChangePasswordCommandResponse>;
