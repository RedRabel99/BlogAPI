using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Shared;

public class TagPostQueryFilter(string? tagName) : IQueryFilter<Tag>
{
    private readonly string? _tagName = tagName;
    public IQueryable<Tag> Apply(IQueryable<Tag> query)
    {
        if (!string.IsNullOrWhiteSpace(_tagName))
        {
            query = query.Where(t => t.TagName.Contains(_tagName));
        }

        return query;
    }
}
