namespace BlogAPI.Application.Shared;

public interface IQueryFilter<T> where T : class
{
    IQueryable<T> Apply(IQueryable<T> query);
}
