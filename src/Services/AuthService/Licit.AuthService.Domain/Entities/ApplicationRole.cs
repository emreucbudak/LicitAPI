using Licit.AuthService.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>, IHasTimestamps
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
