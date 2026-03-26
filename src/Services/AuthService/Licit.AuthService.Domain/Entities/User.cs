using Microsoft.AspNetCore.Identity;

namespace Licit.AuthService.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>, IHasTimestamps
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
