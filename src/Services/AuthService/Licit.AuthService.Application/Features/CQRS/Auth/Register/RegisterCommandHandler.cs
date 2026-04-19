using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.Common;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.Register.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Register;

public class RegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IPasswordHasher<ApplicationUser> passwordHasher,
    IRegisterVerificationStore registerVerificationStore,
    ILoginEmailPublisher loginEmailPublisher,
    AuthVerificationSettings authVerificationSettings,
    IValidator<RegisterCommandRequest> validator) : IRequestHandler<RegisterCommandRequest, RegisterCommandResponse>
{
    public async Task<RegisterCommandResponse> Handle(RegisterCommandRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new EmailAlreadyExistsException();

        var email = request.Email.Trim();
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var challengeId = Guid.NewGuid().ToString("N");
        var verificationCode = VerificationCodeHelper.GenerateSixDigitCode();
        var expiresAt = DateTime.UtcNow.AddMinutes(authVerificationSettings.RegisterVerificationCodeExpirationMinutes);
        var lifetime = expiresAt - DateTime.UtcNow;
        var temporaryToken = tokenService.GenerateTemporaryRegisterToken(email, expiresAt, challengeId);
        var pendingUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };
        var passwordHash = passwordHasher.HashPassword(pendingUser, request.Password);

        await registerVerificationStore.StoreAsync(
            temporaryToken,
            new PendingRegistrationVerification
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = passwordHash,
                Code = verificationCode,
                ChallengeId = challengeId,
                ExpiresAtUtc = expiresAt,
                RemainingAttempts = authVerificationSettings.MaxVerificationAttempts
            },
            lifetime,
            cancellationToken);

        try
        {
            await loginEmailPublisher.PublishRegisterVerificationCodeAsync(
                email,
                verificationCode,
                expiresAt,
                $"{firstName} {lastName}".Trim(),
                cancellationToken);
        }
        catch
        {
            await registerVerificationStore.RemoveAsync(temporaryToken, cancellationToken);
            throw;
        }

        return new RegisterCommandResponse(temporaryToken, expiresAt, email);
    }
}
