using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.Common;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.ChangePassword.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.ChangePassword;

public class ChangePasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IPasswordHistoryRepository passwordHistoryRepository,
    IUserPasswordBloomService userPasswordBloomService,
    IPasswordFingerprintService passwordFingerprintService,
    IPasswordHasher<ApplicationUser> passwordHasher,
    ICurrentUserService currentUserService,
    IValidator<ChangePasswordCommandRequest> validator) : IRequestHandler<ChangePasswordCommandRequest, ChangePasswordCommandResponse>
{
    public async Task<ChangePasswordCommandResponse> Handle(ChangePasswordCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("Gecersiz kullanici oturumu.");

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedException("Gecersiz kullanici oturumu.");

        if (!IsPasswordMatch(passwordHasher, user, user.PasswordHash, request.CurrentPassword))
            throw new CurrentPasswordInvalidException();

        var historyEntries = await passwordHistoryRepository.GetLatestByUserIdAsync(user.Id, 3, cancellationToken);
        var existingFingerprints = await userPasswordBloomService.GetFingerprintsAsync(user.Id, cancellationToken);
        var newFingerprint = passwordFingerprintService.CreateFingerprint(request.NewPassword);
        var bloomMayContain = await userPasswordBloomService.MayContainAsync(user.Id, newFingerprint, cancellationToken);

        if (PasswordReuseHelper.ShouldCheckHashes(user, historyEntries, existingFingerprints, bloomMayContain)
            && PasswordReuseHelper.MatchesCurrentOrHistory(user, request.NewPassword, historyEntries, passwordHasher))
            throw new PasswordReuseNotAllowedException();

        var previousPasswordHash = user.PasswordHash;
        var previousCurrentFingerprint = user.CurrentPasswordFingerprint;
        user.CurrentPasswordFingerprint = newFingerprint;

        var changeResult = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changeResult.Succeeded)
        {
            user.CurrentPasswordFingerprint = previousCurrentFingerprint;
            throw new BusinessRuleException(string.Join(", ", changeResult.Errors.Select(error => error.Description)));
        }

        await MaintainPasswordHistoryAsync(user.Id, previousPasswordHash, historyEntries, cancellationToken);

        await userPasswordBloomService.SetFingerprintsAsync(
            user.Id,
            PasswordReuseHelper.BuildFingerprintWindow(
                newFingerprint,
                previousCurrentFingerprint,
                NormalizeFingerprintsForRotation(existingFingerprints, previousCurrentFingerprint)),
            cancellationToken);

        return new ChangePasswordCommandResponse(true);
    }

    private async Task MaintainPasswordHistoryAsync(
        Guid userId,
        string? previousPasswordHash,
        IReadOnlyList<PasswordHistory> historyEntries,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(previousPasswordHash))
        {
            await passwordHistoryRepository.AddAsync(
                new PasswordHistory
                {
                    Id = Guid.CreateVersion7(),
                    UserId = userId,
                    PasswordHash = previousPasswordHash
                },
                cancellationToken);
        }

        if (historyEntries.Count >= 3)
            passwordHistoryRepository.RemoveRange(new[] { historyEntries.Last() });

        await passwordHistoryRepository.SaveChangesAsync(cancellationToken);
    }

    private static bool IsPasswordMatch(
        IPasswordHasher<ApplicationUser> passwordHasher,
        ApplicationUser user,
        string? passwordHash,
        string password)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return false;

        var verificationResult = passwordHasher.VerifyHashedPassword(user, passwordHash, password);
        return verificationResult is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    private static IReadOnlyList<string> NormalizeFingerprintsForRotation(
        IReadOnlyList<string> existingFingerprints,
        string? previousCurrentFingerprint)
    {
        if (string.IsNullOrWhiteSpace(previousCurrentFingerprint))
            return existingFingerprints;

        if (existingFingerprints.Count > 0
            && string.Equals(existingFingerprints[0], previousCurrentFingerprint, StringComparison.Ordinal))
        {
            return existingFingerprints;
        }

        return new[] { previousCurrentFingerprint }
            .Concat(existingFingerprints)
            .ToArray();
    }
}
