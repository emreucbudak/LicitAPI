using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.ForgotPassword;

public record ForgotPasswordCommandRequest(string Email) : IRequest<ForgotPasswordCommandResponse>;
