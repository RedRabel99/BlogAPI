using BlogAPI.Application.DTOs.Comments;
using BlogAPI.Application.Posts.Dtos;
using BlogAPI.Domain.Entities;
using System.Linq.Expressions;

namespace BlogAPI.Application.Mapping;

public static class CommentMappers
{
    public static CommentDto ToDto(this Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            Author = new AuthorDto { Id = comment.UserProfile.Id, Username = comment.UserProfile.Username},
            PostId = comment.PostId,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
        };
    }

    public static Expression<Func<Comment, CommentDto>> ProjectToDto =>
        comment => new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            Author = new AuthorDto { Id = comment.UserProfile.Id, Username = comment.UserProfile.Username },
            PostId = comment.PostId,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
        };
}
