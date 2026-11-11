using AegisFlow.Users.Api.Contracts;
using AegisFlow.Users.Api.Data;
using AegisFlow.Users.Api.Models;
using AegisFlow.Users.Api.Validation;
using AegisFlow.ServiceDefaults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Users.Api.Endpoints;

public static class UserEndpoints
{
    private const int MaxPageSize = 100;

    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // All routes live under /api/users, so an Ingress can route by path prefix later.
        var users = app.MapGroup("/api/users").WithTags("Users");

        users.MapGet("/", ListUsers).WithName("ListUsers");
        users.MapGet("/{id:guid}", GetUser).WithName("GetUser");
        users.MapPost("/", CreateUser).WithName("CreateUser");
        users.MapPut("/{id:guid}", UpdateUser).WithName("UpdateUser");
        users.MapDelete("/{id:guid}", DeleteUser).WithName("DeleteUser");

        return app;
    }

    // GET /api/users?page=1&pageSize=20
    private static async Task<Ok<PagedResponse<UserResponse>>> ListUsers(
        UsersDbContext db, CancellationToken ct, int page = 1, int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = db.Users.AsNoTracking();
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.CreatedAtUtc).ThenBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserResponse(u.Id, u.Email, u.FullName, u.CreatedAtUtc, u.UpdatedAtUtc))
            .ToListAsync(ct);

        return TypedResults.Ok(new PagedResponse<UserResponse>(items, page, pageSize, totalCount));
    }

    // GET /api/users/{id}
    private static async Task<Results<Ok<UserResponse>, NotFound>> GetUser(
        Guid id, UsersDbContext db, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        return user is null ? TypedResults.NotFound() : TypedResults.Ok(UserResponse.From(user));
    }

    // POST /api/users
    private static async Task<Results<Created<UserResponse>, ValidationProblem, ProblemHttpResult>> CreateUser(
        CreateUserRequest request, UsersDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = UserValidator.Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        // Emails are stored lower-case, so "Ada@Example.com" and "ada@example.com" are the same user.
        var email = request.Email!.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email, ct))
        {
            return TypedResults.Problem(
                title: "A user with this email already exists.",
                statusCode: StatusCodes.Status409Conflict);
        }

        var user = new User
        {
            Email = email,
            FullName = request.FullName!.Trim(),
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created($"/api/users/{user.Id}", UserResponse.From(user));
    }

    // PUT /api/users/{id}
    private static async Task<Results<Ok<UserResponse>, NotFound, ValidationProblem>> UpdateUser(
        Guid id, UpdateUserRequest request, UsersDbContext db, TimeProvider clock, CancellationToken ct)
    {
        var errors = UserValidator.Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        user.FullName = request.FullName!.Trim();
        user.UpdatedAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(UserResponse.From(user));
    }

    // DELETE /api/users/{id}
    private static async Task<Results<NoContent, NotFound>> DeleteUser(
        Guid id, UsersDbContext db, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }
}
