using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Common;

public static class PasswordReuseHelper
{
    public static bool MatchesCurrentOrHistory(
        ApplicationUser user,
        string candidatePassword,
        IEnumerable<PasswordHistory> historyEntries,
        IPasswordHasher<ApplicationUser> passwordHasher)
    {
        var currentMatches = !string.IsNullOrWhiteSpace(user.PasswordHash)
            && passwordHasher.VerifyHashedPassword(user, user.PasswordHash, candidatePassword) != PasswordVerificationResult.Failed;

        if (currentMatches)
            return true;

        return historyEntries.Any(history =>
            passwordHasher.VerifyHashedPassword(user, history.PasswordHash, candidatePassword) != PasswordVerificationResult.Failed);
    }
}
