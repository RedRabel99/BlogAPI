using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Application.Shared.Pagination;

public class PagedList<TValue>
{
    private PagedList(List<TValue> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public List<TValue> Items { get;}
    public int Page { get;}
    public int PageSize{ get;}
    public int TotalCount { get;}
    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;

    public static async Task<PagedList<TValue>> CreateAsync(IQueryable<TValue> query, int page, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var result = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new(result, page, pageSize, totalCount);
    }
}
