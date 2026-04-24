using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Shared;

public class TagPostQueryFilter(string? slug) : IQueryFilter<Tag>
{
    private readonly string? _slug = slug;
    public IQueryable<Tag> Apply(IQueryable<Tag> query)
    {
        if (!string.IsNullOrWhiteSpace(_slug))
        {
            query = query.Where(t => t.Slug.Contains(_slug));
        }

        return query;
    }
}
