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

        var email = request.Email.Trim();
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
            throw new EmailAlreadyExistsException();

        var verificationCode = VerificationCodeHelper.GenerateSixDigitCode();
        var expiresAt = DateTime.UtcNow.AddMinutes(authVerificationSettings.RegisterVerificationCodeExpirationMinutes);
        var lifetime = expiresAt - DateTime.UtcNow;
        var pendingUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };
        var passwordHash = passwordHasher.HashPassword(pendingUser, request.Password);

        await registerVerificationStore.StoreAsync(
            email,
            new PendingRegistrationVerification
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = passwordHash,
                Code = verificationCode,
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
            await registerVerificationStore.RemoveAsync(email, cancellationToken);
            throw;
        }

        return new RegisterCommandResponse(email, expiresAt);
    }
}
