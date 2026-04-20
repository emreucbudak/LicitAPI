using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Common;

public static class PasswordReuseHelper
{
    public static bool ShouldCheckHashes(
        ApplicationUser user,
        IReadOnlyCollection<PasswordHistory> historyEntries,
        IReadOnlyCollection<string> exactFingerprints,
        bool bloomMayContain)
    {
        if (bloomMayContain)
            return true;

        if (string.IsNullOrWhiteSpace(user.CurrentPasswordFingerprint))
            return true;

        var expectedFingerprintCount = Math.Min(4, historyEntries.Count + 1);
        return exactFingerprints.Count < expectedFingerprintCount;
    }

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

    public static IReadOnlyList<string> BuildFingerprintWindow(
        string newCurrentFingerprint,
        string? previousCurrentFingerprint,
        IReadOnlyList<string> existingFingerprints)
    {
        var updatedFingerprints = new List<string> { newCurrentFingerprint };

        if (!string.IsNullOrWhiteSpace(previousCurrentFingerprint))
            updatedFingerprints.Add(previousCurrentFingerprint);

        if (existingFingerprints.Count > 1)
            updatedFingerprints.AddRange(existingFingerprints.Skip(1));

        return updatedFingerprints
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .Take(4)
            .ToArray();
    }
}
