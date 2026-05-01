using FlashMediator;
using Licit.AuthService.Application.Constants;
using Licit.AuthService.Application.Features.CQRS.Auth.ChangePassword;
using Licit.AuthService.Application.Features.CQRS.Auth.ForgotPassword;
using Licit.AuthService.Application.Features.CQRS.Auth.GetProfile;
using Licit.AuthService.Application.Features.CQRS.Auth.Login;
using Licit.AuthService.Application.Features.CQRS.Auth.RefreshToken;
using Licit.AuthService.Application.Features.CQRS.Auth.Register;
using Licit.AuthService.Application.Features.CQRS.Auth.ResetForgotPassword;
using Licit.AuthService.Application.Features.CQRS.Auth.RevokeToken;
using Licit.AuthService.Application.Features.CQRS.Auth.UpdateProfile;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyForgotPassword;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyLogin;
using Licit.AuthService.Application.Features.CQRS.Auth.VerifyRegister;
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
    public async Task<IActionResult> VerifyLogin([FromBody] VerifyLoginCommandRequest request)
    {
        var result = await mediator.Send(request);
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
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommandRequest request)
    {
        var result = await mediator.Send(request);
        return Ok(result);
    }

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
        var profile = await mediator.Send(new GetProfileQueryRequest());
        return Ok(profile);
    }

    [Authorize(Policy = AuthPolicies.AccessToken)]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileCommandRequest request)
    {
        var profile = await mediator.Send(request);
        return Ok(profile);
    }
}
