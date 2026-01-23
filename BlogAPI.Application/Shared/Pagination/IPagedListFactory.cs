using Microsoft.Extensions.Options;

namespace BlogAPI.Application.Shared.Pagination;

public interface IPagedListFactory
{
    public Task<PagedList<T>> CreateAsync<T>(
        IQueryable<T> query, int? page, int? pageSize);
}
