namespace Licit.AuthService.Application.DTOs;

public sealed record LoginVerificationChallenge(
    string Code,
    string ChallengeId
);
