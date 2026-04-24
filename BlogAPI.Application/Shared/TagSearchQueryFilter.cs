using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Shared;

public class TagSearchQueryFilter(string? tagName) : IQueryFilter<Tag>
{
    private string? TagName = tagName;
    public IQueryable<Tag> Apply(IQueryable<Tag> query)
    {
        if (!string.IsNullOrWhiteSpace(TagName))
        {
            query = query.Where(t => t.TagName.Contains(TagName));
        }

        return query;
    }
}
