using BlogAPI.Application.DTOs.Posts;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Shared;

public class PostSearchQueryFilter(PostQueryParametersDto? parameters) : IQueryFilter<Post>
{
    private readonly string? _title = parameters?.Title;
    private readonly List<string> _usernames = parameters?.Usernames ?? new List<string>();
    private readonly DateOnly? _from = parameters?.From;
    private readonly DateOnly? _to = parameters?.To;
    private readonly List<string> _tags = parameters?.Tags ?? new List<string>();

    public IQueryable<Post> Apply(IQueryable<Post> query)
    {
        if (!string.IsNullOrWhiteSpace(_title))
            query = query.Where(p => p.Title.Contains(_title));

        if (_usernames.Count > 0)
            query = query.Where(p => _usernames.Contains(p.UserProfile.Username));

        if (_from.HasValue)
            query = query.Where(p => p.CreatedAt >= _from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

        if (_to.HasValue)
            query = query.Where(p => p.CreatedAt <= _to.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));

        if (_tags.Count > 0)
        {
            var lowerTags = _tags.Select(t => t.ToLower()).ToList();
            query = query.Where(p => p.Tags.Any(t => lowerTags.Contains(t.TagName.ToLower())));
        }

        return query;
    }
}