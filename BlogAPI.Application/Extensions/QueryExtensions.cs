using System;
using System.Collections.Generic;
using System.Text;
using BlogAPI.Application.Shared;


namespace BlogAPI.Application.Extensions;

public static class QueryExtensions
{
    public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, IQueryFilter<T> filter) where T : class 
        => filter.Apply(query);

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, IQuerySorting<T> sorting) where T : class
    {
        if (string.IsNullOrEmpty(sorting.SortColumn))
        {
            return query;
        }

        var keySelector = sorting.GetSortProperty();

        return sorting.SortOrder == SortingOrder.Ascending
            ? query.OrderBy(keySelector)
            : query.OrderByDescending(keySelector);
    }
}
