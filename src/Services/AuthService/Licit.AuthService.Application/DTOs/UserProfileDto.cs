namespace Licit.AuthService.Application.DTOs;

public record UserProfileDto(
    string? Id,
    string? Email,
    string? Role,
    string? FirstName,
    string? LastName
);
