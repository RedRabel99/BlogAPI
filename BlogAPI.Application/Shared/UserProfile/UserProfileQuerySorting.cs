using System.Linq.Expressions;
using BlogAPI.Application.Extensions;

namespace BlogAPI.Application.Shared.UserProfile;

public class UserProfileQuerySorting(string? sortOrder, string? sortColumn) : IQuerySorting<Domain.Entities.UserProfile>
{
    private SortingOrder SortOrder => sortOrder?.ToLower() switch
    {
        "asc" => SortingOrder.Ascending,
        "desc" => SortingOrder.Descending,
        _ => SortingOrder.Ascending
    };
    private string? SortColumn { get; set; } = sortColumn;

    private Expression<Func<Domain.Entities.UserProfile, object>> SortProperty =>
     SortColumn?.ToLower() switch
     {
            "Username" => u => u.Username,
            _ => u => u.Id
     };

    public IQueryable<Domain.Entities.UserProfile> Apply(IQueryable<Domain.Entities.UserProfile> query)
    {
        return query.OrderByDirection(SortProperty, SortOrder);
    }
}