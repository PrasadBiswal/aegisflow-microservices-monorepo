using AegisFlow.Users.Api.Models;

namespace AegisFlow.Users.Api.Contracts;

// The API's public shapes. Kept separate from the database model (User),
// so the table can change without breaking API clients.

// Fields are nullable because a client can omit them; UserValidator rejects missing values.
public sealed record CreateUserRequest(string? Email, string? FullName);

public sealed record UpdateUserRequest(string? FullName);

public sealed record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc)
{
    public static UserResponse From(User user) =>
        new(user.Id, user.Email, user.FullName, user.CreatedAtUtc, user.UpdatedAtUtc);
}
