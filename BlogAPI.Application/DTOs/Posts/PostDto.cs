namespace BlogAPI.Application.DTOs.Posts;

public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public Guid UserProfileId { get; set; }
    public string Slug { get; internal set; }
    public string Excerpt { get; internal set; }
    public string Content { get; internal set; }
    public List<string> Tags { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
}
