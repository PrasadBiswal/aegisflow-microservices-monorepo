namespace AegisFlow.Notifications.Api.Models;

public enum NotificationChannel { Email, Sms }

public enum NotificationStatus { Queued, Sent }

// A message to send. Delivery through a real provider (e.g. SES / Twilio) is added later.
public sealed class Notification
{
    public const int RecipientMaxLength = 256;
    public const int SubjectMaxLength = 200;
    public const int BodyMaxLength = 4000;

    public Guid Id { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; } = NotificationStatus.Queued;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
}
