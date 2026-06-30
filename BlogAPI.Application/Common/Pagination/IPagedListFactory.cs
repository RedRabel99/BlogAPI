using Microsoft.Extensions.Options;

namespace BlogAPI.Application.Common.Pagination;

public interface IPagedListFactory
{
    public Task<PagedList<T>> CreateAsync<T>(
        IQueryable<T> query, int? page, int? pageSize);
}
