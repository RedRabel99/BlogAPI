using System.Linq.Expressions;
using BlogAPI.Application.Extensions;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Shared;

public class CommentQuerySorting(string? sortOrder, string? sortColumn) : IQuerySorting<Comment>
{
    private readonly Expression<Func<Comment, object>> _sortProperty = sortColumn?.ToLower() switch
    {
        "createdat" => c => c.CreatedAt,
        "updatedat" => c => c.UpdatedAt,
        _ => c => c.CreatedAt
    };

    private readonly SortingOrder _sortOrder = sortOrder?.ToLower() switch
    {
        "asc" => SortingOrder.Ascending,
        _ => SortingOrder.Descending
    };

    public IQueryable<Comment> Apply(IQueryable<Comment> query)
        => query.OrderByDirection(_sortProperty, _sortOrder);
}
