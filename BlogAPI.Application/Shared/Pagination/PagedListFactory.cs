using Microsoft.Extensions.Options;

namespace BlogAPI.Application.Shared.Pagination;

public class PagedListFactory : IPagedListFactory
{
    private readonly PaginationOptions _options;
    public PagedListFactory(IOptions<PaginationOptions> options)
    {
        _options = options.Value;
    }
    public async Task<PagedList<T>> CreateAsync<T>(
        IQueryable<T> query, int? page, int? pageSize)
    {
        var correctPageSize = pageSize ?? _options.DefaultPageSize;
        var correctPage = page ?? _options.DefaultPage;

        return await PagedList<T>.CreateAsync(query, correctPage, correctPageSize);
    }
}
