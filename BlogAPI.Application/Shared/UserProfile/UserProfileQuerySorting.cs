using System.Linq.Expressions;

namespace BlogAPI.Application.Shared.UserProfile;

public class UserProfileQuerySorting(string sortOrder, string? sortColumn) : IQuerySorting<Domain.Entities.UserProfile>
{
    public SortingOrder SortOrder { get; set; } =
        sortOrder.Equals("asc") ? SortingOrder.Ascending : SortingOrder.Descending;

    public string? SortColumn { get; set; } = sortColumn;

    public Expression<Func<Domain.Entities.UserProfile, object>> GetSortProperty()
    {
        return SortColumn?.ToLower() switch
        {
            "Username" => u => u.Username,
            _ => u => u.Id
        };

    }
}