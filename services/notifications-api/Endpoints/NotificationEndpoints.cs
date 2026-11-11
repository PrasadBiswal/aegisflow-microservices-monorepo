using AegisFlow.Notifications.Api.Contracts;
using AegisFlow.Notifications.Api.Data;
using AegisFlow.Notifications.Api.Models;
using AegisFlow.ServiceDefaults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Notifications.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var notifications = app.MapGroup("/api/notifications").WithTags("Notifications");
        notifications.MapGet("/", List).WithName("ListNotifications");
        notifications.MapGet("/{id:guid}", Get).WithName("GetNotification");
        notifications.MapPost("/", Create).WithName("CreateNotification");
        notifications.MapPost("/{id:guid}/send", Send).WithName("SendNotification");
        return app;
    }

    // GET /api/notifications?recipient=...&page=1&pageSize=20
    private static async Task<Ok<PagedResponse<NotificationResponse>>> List(
        NotificationsDbContext db, CancellationToken ct, string? recipient = null, int page = 1, int pageSize = 20)
    {
        var query = db.Notifications.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(recipient))
        {
            var value = recipient.Trim();
            query = query.Where(n => n.Recipient == value);
        }

        var result = await query.OrderByDescending(n => n.CreatedAtUtc).ThenBy(n => n.Id).ToPagedResponseAsync(page, pageSize, ct);
        return TypedResults.Ok(result.Map(NotificationResponse.From));
    }

    private static async Task<Results<Ok<NotificationResponse>, NotFound>> Get(Guid id, NotificationsDbContext db, CancellationToken ct)
    {
        var notification = await db.Notifications.AsNoTracking().FirstOrDefaultAsync(n => n.Id == id, ct);
        return notification is null ? TypedResults.NotFound() : TypedResults.Ok(NotificationResponse.From(notification));
    }

    // 202 Accepted: the notification is queued; sending happens separately.
    private static async Task<Results<Accepted<NotificationResponse>, ValidationProblem>> Create(
        CreateNotificationRequest request, NotificationsDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = request.Validate();
        if (errors.HasErrors) return TypedResults.ValidationProblem(errors.ToDictionary());

        var notification = new Notification
        {
            Channel = Enum.Parse<NotificationChannel>(request.Channel!, ignoreCase: true),
            Recipient = request.Recipient!.Trim(),
            Subject = request.Subject?.Trim(),
            Body = request.Body!.Trim(),
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
        };
        db.Notifications.Add(notification);
        await db.SaveChangesAsync(ct);
        return TypedResults.Accepted($"/api/notifications/{notification.Id}", NotificationResponse.From(notification));
    }

    // Marks a queued notification as sent. A notification can be sent only once.
    private static async Task<Results<Ok<NotificationResponse>, NotFound, ProblemHttpResult>> Send(
        Guid id, NotificationsDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (notification is null) return TypedResults.NotFound();
        if (notification.Status == NotificationStatus.Sent)
            return TypedResults.Problem(title: "This notification was already sent.", statusCode: StatusCodes.Status409Conflict);

        notification.Status = NotificationStatus.Sent;
        notification.SentAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(NotificationResponse.From(notification));
    }
}
