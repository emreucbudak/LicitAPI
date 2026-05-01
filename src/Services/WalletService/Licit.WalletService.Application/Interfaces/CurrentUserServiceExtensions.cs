using Licit.WalletService.Application.Exceptions;

namespace Licit.WalletService.Application.Interfaces;

internal static class CurrentUserServiceExtensions
{
    public static Guid GetRequiredUserId(this ICurrentUserService currentUserService) =>
        currentUserService.UserId ?? throw new UnauthorizedException("Kullanici kimligi bulunamadi.");
}
