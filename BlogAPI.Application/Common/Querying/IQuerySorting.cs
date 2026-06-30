namespace BlogAPI.Application.Common.Querying;

public interface IQuerySorting<T> where T : class
{
    IQueryable<T> Apply(IQueryable<T> query);
}
