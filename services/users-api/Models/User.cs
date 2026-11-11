namespace AegisFlow.Users.Api.Models;

public sealed class User
{
    // Shared by the database schema and request validation, so both always agree.
    public const int EmailMaxLength = 256;
    public const int FullNameMaxLength = 200;

    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
