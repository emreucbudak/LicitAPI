namespace Licit.AuthService.Application.Features.CQRS.Auth.VerifyRegister;

public record VerifyRegisterCommandResponse(
    bool IsVerified
);
