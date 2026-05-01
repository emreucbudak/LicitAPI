using Licit.AuthService.Application.Exceptions;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.RefreshToken.Exceptions;

public class UserFromTokenNotFoundException : NotFoundException
{
    public UserFromTokenNotFoundException(Guid userId)
        : base("Kullanıcı", userId) { }
}
