using System.Linq.Expressions;
using BlogAPI.Application.Extensions;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Shared;

public class PostQuerySorting(string? sortOrder, string? sortColumn) : IQuerySorting<Post>
{
    private readonly Expression<Func<Post, object>> _sortProperty = sortColumn?.ToLower() switch
    {
        "title" => p => p.Title,
        "username" => p => p.UserProfile.Username,
        "createdat" => p => p.CreatedAt,
        "updatedat" => p => p.UpdatedAt,
        _ => p => p.CreatedAt
    };  

    private readonly SortingOrder _sortOrder = sortOrder?.ToLower() switch
    {
        "desc" => SortingOrder.Descending,
        _ => SortingOrder.Ascending
    };

    public IQueryable<Post> Apply(IQueryable<Post> query)
        => query.OrderByDirection(_sortProperty, _sortOrder);
}