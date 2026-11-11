namespace AegisFlow.Reviews.Api.Models;

public sealed class Review
{
    public const int TitleMaxLength = 200;
    public const int CommentMaxLength = 2000;

    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
