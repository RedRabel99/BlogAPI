using System.Linq.Expressions;
using BlogAPI.Application.Shared;


namespace BlogAPI.Application.Extensions;

public static class QueryExtensions
{
    extension<T>(IQueryable<T> query) where T : class
    {
        public IQueryable<T> ApplyFiltering(IQueryFilter<T> filter) => filter.Apply(query);
        public IQueryable<T> ApplySorting(IQuerySorting<T> sorting) => sorting.Apply(query);
    }

    public static IQueryable<T> OrderByDirection<T>(
        this IQueryable<T> query,
        Expression<Func<T, object>> keySelector,
        SortingOrder sortingOrder)
        => sortingOrder == SortingOrder.Ascending
            ? query.OrderBy(keySelector)
            : query.OrderByDescending(keySelector);
}
