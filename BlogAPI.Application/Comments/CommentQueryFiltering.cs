using BlogAPI.Application.Comments.Dtos;
using BlogAPI.Domain.Entities;
using BlogAPI.Application.Common.Querying;

namespace BlogAPI.Application.Comments;

public class CommentQueryFiltering(CommentQueryParametersDto? parameters) : IQueryFilter<Comment>
{
    private readonly string? _author = parameters?.Author;
    private readonly DateOnly? _from = parameters?.From;
    private readonly DateOnly? _to = parameters?.To;

    public IQueryable<Comment> Apply(IQueryable<Comment> query)
    {
        if (!string.IsNullOrWhiteSpace(_author))
            query = query.Where(c => c.UserProfile.Username == _author);

        if (_from.HasValue)
            query = query.Where(c => c.CreatedAt >= _from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

        if (_to.HasValue)
            query = query.Where(c => c.CreatedAt <= _to.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));

        return query;
    }
}
