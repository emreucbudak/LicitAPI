using FlashMediator;
using FluentValidation;
using Licit.AuthService.Application.Exceptions;
using Licit.AuthService.Application.Features.CQRS.Auth.Commands.ChangePassword.Exceptions;
using Licit.AuthService.Application.Interfaces;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Application.Features.CQRS.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
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

        if (!await userManager.CheckPasswordAsync(user, request.CurrentPassword))
            throw new CurrentPasswordInvalidException();

        var changeResult = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changeResult.Succeeded)
            throw new BusinessRuleException(string.Join(", ", changeResult.Errors.Select(error => error.Description)));

        return new ChangePasswordCommandResponse(true);
    }
}
