using System.Linq.Expressions;

namespace BlogAPI.Application.Shared;

public interface IQuerySorting<T> where T : class
{
    IQueryable<T> Apply(IQueryable<T> query);
}
