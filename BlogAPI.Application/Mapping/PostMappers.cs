using BlogAPI.Application.DTOs.Posts;
using BlogAPI.Domain.Entities;
using System.Linq.Expressions;

namespace BlogAPI.Application.Mapping;

public static  class PostMappers
{
    public static PostDto ToDto(this Post post)
    {
        return new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Excerpt = post.Excerpt,
            Content = post.Content,
            Author = new AuthorDto { Id = post.UserProfile.Id, Username = post.UserProfile.Username},
            Tags = post.Tags.Select(t => t.TagName).ToList(),
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    public static Expression<Func<Post, PostDto>> ProjectToDto =>
        post => new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Excerpt = post.Excerpt,
            Content = post.Content,
            Author = new AuthorDto { Id = post.UserProfile.Id, Username = post.UserProfile.Username },
            Tags = post.Tags.Select(t => t.TagName).ToList(),
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
}
