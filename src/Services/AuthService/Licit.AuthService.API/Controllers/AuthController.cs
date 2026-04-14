using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlashMediator;
using Licit.AuthService.Application.Constants;
using Licit.AuthService.Application.DTOs;
using Licit.AuthService.Application.Features.CQRS.Auth.Login;
using Licit.AuthService.Application.Features.CQRS.Auth.RefreshToken;
using Licit.AuthService.Application.Features.CQRS.Auth.Register;
using Licit.AuthService.Application.Features.CQRS.Auth.RevokeToken;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyLogin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Licit.AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommandRequest request)
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

    [Authorize(Policy = AuthPolicies.AccessToken)]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RevokeTokenCommandRequest request)
    {
        await mediator.Send(request);
        return NoContent();
    }

    [Authorize(Policy = AuthPolicies.AccessToken)]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var profile = new UserProfileDto(
            Id: User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value,
            Email: User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                    ?? User.FindFirst("email")?.Value,
            Role: User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
            FirstName: User.FindFirst("firstName")?.Value,
            LastName: User.FindFirst("lastName")?.Value
        );

        return Ok(profile);
    }
}

public record VerifyLoginRequest(string Email, string Code);
