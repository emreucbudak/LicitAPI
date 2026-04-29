using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlashMediator;
using Licit.AuthService.Application.Features.CQRS.Auth.ForgotPassword;
using Licit.AuthService.Application.Features.CQRS.Auth.ChangePassword;
using Licit.AuthService.Application.Constants;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.Login;
using Licit.AuthService.Application.Features.CQRS.Auth.ResetForgotPassword;
using Licit.AuthService.Application.Features.CQRS.Auth.RefreshToken;
using Licit.AuthService.Application.Features.CQRS.Auth.Register;
using Licit.AuthService.Application.Features.CQRS.Auth.RevokeToken;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyForgotPassword;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyLogin;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyRegister;
using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Licit.AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommandRequest request)
    {
        var result = await mediator.Send(request);
        return Ok(result);
    }

    [EnableRateLimiting("auth")]
    [HttpPost("register/verify")]
    public async Task<IActionResult> VerifyRegister([FromBody] VerifyRegisterCommandRequest request)
    {
        var result = await mediator.Send(request);
        return Ok(result);
    }

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommandRequest request)
    {
        var result = await mediator.Send(request);
        return Ok(result);
    }

    [EnableRateLimiting("auth")]
    [Authorize(Policy = AuthPolicies.PendingTwoFactor)]
    [HttpPost("login/verify")]
    public async Task<IActionResult> VerifyLogin([FromBody] VerifyLoginRequest request)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst("sub")?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? User.FindFirst("email")?.Value;
        var tokenId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value
            ?? User.FindFirst("jti")?.Value;

        if (!Guid.TryParse(userIdValue, out var userId)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(tokenId))
            return Unauthorized();

        var result = await mediator.Send(new VerifyLoginCommandRequest(
            request.Email,
            request.Code,
            userId,
            email,
            tokenId));

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommandRequest request)
    {
        var result = await mediator.Send(request);
        return Ok(result);
    }

    [EnableRateLimiting("auth")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommandRequest request)
    {
        var result = await mediator.Send(request);
        return Ok(result);
    }

    [EnableRateLimiting("auth")]
    [HttpPost("forgot-password/verify")]
    public async Task<IActionResult> VerifyForgotPassword([FromBody] VerifyForgotPasswordCommandRequest request)
    {
        var result = await mediator.Send(request);
        return Ok(result);
    }

    [EnableRateLimiting("auth")]
    [HttpPost("forgot-password/reset")]
    public async Task<IActionResult> ResetForgotPassword([FromBody] ResetForgotPasswordCommandRequest request)
    {
        var result = await mediator.Send(request);
        return Ok(result);
    }

    [EnableRateLimiting("auth")]
    [Authorize(Policy = AuthPolicies.AccessToken)]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var result = await mediator.Send(new ChangePasswordCommandRequest(
            userId,
            request.CurrentPassword,
            request.NewPassword));

        return Ok(result);
    }

    [Authorize(Policy = AuthPolicies.AccessToken)]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenCommandRequest request)
    {
        await mediator.Send(request);
        return NoContent();
    }

    [Authorize(Policy = AuthPolicies.AccessToken)]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
            return Unauthorized();

        var profile = await CreateUserProfileDtoAsync(user);

        return Ok(profile);
    }

    [Authorize(Policy = AuthPolicies.AccessToken)]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        if (!TryValidateProfileRequest(request, out var firstName, out var lastName))
            return ValidationProblem(ModelState);

        var user = await GetCurrentUserAsync();
        if (user is null)
            return Unauthorized();

        user.FirstName = firstName;
        user.LastName = lastName;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);

            return ValidationProblem(ModelState);
        }

        var profile = await CreateUserProfileDtoAsync(user);

        return Ok(profile);
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdValue, out var userId)
            ? await userManager.FindByIdAsync(userId.ToString())
            : null;
    }

    private async Task<UserProfileDto> CreateUserProfileDtoAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        return new UserProfileDto(
            Id: user.Id.ToString(),
            Email: user.Email,
            Role: roles.FirstOrDefault(),
            FirstName: user.FirstName,
            LastName: user.LastName
        );
    }

    private bool TryValidateProfileRequest(UpdateProfileRequest? request, out string firstName, out string lastName)
    {
        firstName = request?.FirstName?.Trim() ?? string.Empty;
        lastName = request?.LastName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(firstName))
            ModelState.AddModelError(nameof(UpdateProfileRequest.FirstName), "First name is required.");
        else if (firstName.Length > 100)
            ModelState.AddModelError(nameof(UpdateProfileRequest.FirstName), "First name must be 100 characters or fewer.");

        if (string.IsNullOrWhiteSpace(lastName))
            ModelState.AddModelError(nameof(UpdateProfileRequest.LastName), "Last name is required.");
        else if (lastName.Length > 100)
            ModelState.AddModelError(nameof(UpdateProfileRequest.LastName), "Last name must be 100 characters or fewer.");

        return ModelState.IsValid;
    }
}

public record VerifyLoginRequest(string Email, string Code);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record UpdateProfileRequest(string FirstName, string LastName);
