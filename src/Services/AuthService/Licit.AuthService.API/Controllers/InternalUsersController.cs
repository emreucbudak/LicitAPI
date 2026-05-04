using Licit.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Licit.AuthService.API.Controllers;

[ApiController]
[Route("api/auth/internal/users")]
public class InternalUsersController(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : ControllerBase
{
    private const string ServiceKeyHeader = "x-licit-service-key";

    [HttpPost("emails")]
    public async Task<IActionResult> GetEmails(
        [FromBody] InternalUserEmailLookupRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized())
        {
            return Unauthorized();
        }

        var userIds = request.UserIds
            .Where(userId => userId != Guid.Empty)
            .Distinct()
            .ToArray();

        if (userIds.Length == 0)
        {
            return Ok(new InternalUserEmailLookupResponse([]));
        }

        var users = await userManager.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id) && user.Email != null)
            .Select(user => new InternalUserEmailDto(
                user.Id,
                user.Email!,
                user.UserName))
            .ToListAsync(cancellationToken);

        return Ok(new InternalUserEmailLookupResponse(users));
    }

    private bool IsAuthorized()
    {
        var expectedKey = configuration["InternalService:ServiceKey"];
        var providedKey = Request.Headers[ServiceKeyHeader].FirstOrDefault();

        return !string.IsNullOrWhiteSpace(expectedKey)
            && string.Equals(expectedKey, providedKey, StringComparison.Ordinal);
    }
}

public sealed record InternalUserEmailLookupRequest(IReadOnlyCollection<Guid> UserIds);

public sealed record InternalUserEmailDto(
    Guid UserId,
    string Email,
    string? UserName);

public sealed record InternalUserEmailLookupResponse(IReadOnlyList<InternalUserEmailDto> Users);
