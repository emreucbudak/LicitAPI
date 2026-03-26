namespace Licit.AuthService.Domain.Entities;

public interface IHasTimestamps
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
