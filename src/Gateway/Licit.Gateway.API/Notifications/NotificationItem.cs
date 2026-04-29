namespace Licit.Gateway.API.Notifications;

public sealed record NotificationItem(
    string Id,
    string UserId,
    string Type,
    string Title,
    string? Body,
    string? LinkUrl,
    IReadOnlyDictionary<string, string>? Data,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt)
{
    public bool IsRead => ReadAt is not null;
}
