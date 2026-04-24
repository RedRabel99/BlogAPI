using System.Linq.Expressions;

namespace BlogAPI.Application.Shared;

public interface IQuerySorting<T> where T : class
{
    string? SortColumn { get; }
    SortingOrder SortOrder { get; }
    Expression<Func<T, object>> GetSortProperty();
}
