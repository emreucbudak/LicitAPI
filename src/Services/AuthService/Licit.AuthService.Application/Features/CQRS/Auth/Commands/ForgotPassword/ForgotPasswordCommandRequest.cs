using FlashMediator;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommandRequest(string Email) : IRequest<ForgotPasswordCommandResponse>;
