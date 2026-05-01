namespace Licit.AuthService.Application.Features.CQRS.Auth.Queries.GetProfile;

public record GetProfileQueryResponse(
    string? Id,
    string? Email,
    string? Role,
    string? FirstName,
    string? LastName
);
