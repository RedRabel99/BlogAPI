using BlogAPI.Application.DTOs.Posts;

namespace BlogAPI.Application.DTOs.Comments;

public class CommentDto
{
    public object Id { get; internal set; }
    public object Content { get; internal set; }
    public AuthorDto Author { get; internal set; }
    public Guid PostId { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
}
