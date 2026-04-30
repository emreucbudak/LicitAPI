namespace Licit.NotificationService.API.Notifications;

public sealed record NotificationDto(
    string Id,
    string Type,
    string Title,
    string? Body,
    string? LinkUrl,
    IReadOnlyDictionary<string, string>? Data,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt,
    bool IsRead);

public sealed record NotificationFeedResponse(IReadOnlyList<NotificationDto> Items);

public sealed record UnreadCountResponse(int Count);

public sealed record MarkAllReadResponse(int UpdatedCount);

public sealed record PublishNotificationRequest(
    string Type,
    string Title,
    string? Body = null,
    string? LinkUrl = null,
    IReadOnlyDictionary<string, string>? Data = null);

public sealed record CreateNotificationRequest(
    string UserId,
    string Type,
    string Title,
    string? Body = null,
    string? LinkUrl = null,
    IReadOnlyDictionary<string, string>? Data = null);
