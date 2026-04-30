namespace Licit.NotificationService.API.Notifications;

public sealed class NotificationItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string UserId { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public string? LinkUrl { get; set; }

    public Dictionary<string, string>? Data { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ReadAt { get; set; }

    public bool IsRead => ReadAt is not null;
}
