using BlogAPI.Application.DTOs.Posts;

namespace BlogAPI.Application.DTOs.Comments;

public class CommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public AuthorDto Author { get; set; } = null!;
    public Guid PostId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
