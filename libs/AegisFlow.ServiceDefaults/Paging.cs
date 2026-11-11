using Microsoft.EntityFrameworkCore;

namespace AegisFlow.ServiceDefaults;

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public PagedResponse<TResult> Map<TResult>(Func<T, TResult> map) =>
        new(Items.Select(map).ToList(), Page, PageSize, TotalCount);
}

public static class PagingExtensions
{
    public const int MaxPageSize = 100;

    // One COUNT query plus one page query. The source query must already be ordered.
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResponse<T>(items, page, pageSize, totalCount);
    }
}
