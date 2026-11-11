using System.Net.Mail;
using AegisFlow.Notifications.Api.Models;
using AegisFlow.ServiceDefaults;

namespace AegisFlow.Notifications.Api.Contracts;

public sealed record CreateNotificationRequest(string? Channel, string? Recipient, string? Subject, string? Body)
{
    public ValidationErrors Validate()
    {
        var errors = new ValidationErrors()
            .OneOf<NotificationChannel>("channel", Channel)
            .Required("recipient", Recipient, Notification.RecipientMaxLength)
            .MaxLength("subject", Subject, Notification.SubjectMaxLength)
            .Required("body", Body, Notification.BodyMaxLength);

        // Email needs a valid address and a subject; SMS needs a phone number like +14155550100.
        if (ValidationErrors.TryParseEnum<NotificationChannel>(Channel, out var channel) && !string.IsNullOrWhiteSpace(Recipient))
        {
            var recipient = Recipient.Trim();
            if (channel == NotificationChannel.Email)
            {
                errors.Rule(MailAddress.TryCreate(recipient, out _), "recipient", "'recipient' must be an email address.")
                      .Rule(!string.IsNullOrWhiteSpace(Subject), "subject", "'subject' is required for email.");
            }
            else
            {
                errors.Rule(recipient.Length is >= 9 and <= 16 && recipient[0] == '+' && recipient[1..].All(char.IsAsciiDigit),
                    "recipient", "'recipient' must be a phone number like +14155550100.");
            }
        }

        return errors;
    }
}

public sealed record NotificationResponse(
    Guid Id, string Channel, string Recipient, string? Subject, string Body, string Status,
    DateTime CreatedAtUtc, DateTime? SentAtUtc)
{
    public static NotificationResponse From(Notification n) =>
        new(n.Id, n.Channel.ToString(), n.Recipient, n.Subject, n.Body, n.Status.ToString(), n.CreatedAtUtc, n.SentAtUtc);
}
