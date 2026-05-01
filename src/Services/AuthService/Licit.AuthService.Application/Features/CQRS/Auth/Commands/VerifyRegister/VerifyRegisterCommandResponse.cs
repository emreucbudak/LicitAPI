namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.VerifyRegister;

public record VerifyRegisterCommandResponse(
    bool IsVerified
);
