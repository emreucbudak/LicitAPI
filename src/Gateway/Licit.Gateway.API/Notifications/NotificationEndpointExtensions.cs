using Microsoft.AspNetCore.Http.HttpResults;

namespace Licit.Gateway.API.Notifications;

public static class NotificationEndpointExtensions
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications")
            .RequireAuthorization(NotificationAuth.AccessTokenPolicy);

        group.MapGet("/", GetNotificationsAsync);
        group.MapGet("/unread-count", GetUnreadCountAsync);
        group.MapPost("/", PublishNotificationAsync);
        group.MapPatch("/{id}/read", MarkReadAsync);
        group.MapPatch("/read-all", MarkAllReadAsync);

        return endpoints;
    }

    private static async Task<Results<Ok<NotificationFeedResponse>, UnauthorizedHttpResult>> GetNotificationsAsync(
        HttpContext httpContext,
        INotificationService notificationService,
        CancellationToken cancellationToken,
        int take = 50)
    {
        var userId = NotificationUser.ResolveUserId(httpContext.User);
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var boundedTake = Math.Clamp(take, 1, 100);
        var items = await notificationService.GetRecentAsync(userId, boundedTake, cancellationToken);

        return TypedResults.Ok(new NotificationFeedResponse(items));
    }

    private static async Task<Results<Ok<UnreadCountResponse>, UnauthorizedHttpResult>> GetUnreadCountAsync(
        HttpContext httpContext,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var userId = NotificationUser.ResolveUserId(httpContext.User);
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var count = await notificationService.GetUnreadCountAsync(userId, cancellationToken);

        return TypedResults.Ok(new UnreadCountResponse(count));
    }

    private static async Task<Results<Created<NotificationDto>, BadRequest, UnauthorizedHttpResult>> PublishNotificationAsync(
        PublishNotificationRequest request,
        HttpContext httpContext,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var userId = NotificationUser.ResolveUserId(httpContext.User);
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var notification = await notificationService.PublishAsync(
            new CreateNotificationRequest(
                userId,
                request.Type,
                request.Title,
                request.Body,
                request.LinkUrl,
                request.Data),
            cancellationToken);

        if (notification is null)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Created($"/api/notifications/{notification.Id}", notification);
    }

    private static async Task<Results<Ok<NotificationDto>, NotFound, UnauthorizedHttpResult>> MarkReadAsync(
        string id,
        HttpContext httpContext,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var userId = NotificationUser.ResolveUserId(httpContext.User);
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var notification = await notificationService.MarkReadAsync(userId, id, cancellationToken);
        if (notification is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(notification);
    }

    private static async Task<Results<Ok<MarkAllReadResponse>, UnauthorizedHttpResult>> MarkAllReadAsync(
        HttpContext httpContext,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var userId = NotificationUser.ResolveUserId(httpContext.User);
        if (userId is null)
        {
            return TypedResults.Unauthorized();
        }

        var updatedCount = await notificationService.MarkAllReadAsync(userId, cancellationToken);

        return TypedResults.Ok(new MarkAllReadResponse(updatedCount));
    }
}
