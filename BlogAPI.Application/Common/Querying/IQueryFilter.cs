namespace BlogAPI.Application.Common.Querying;

public interface IQueryFilter<T> where T : class
{
    IQueryable<T> Apply(IQueryable<T> query);
}
