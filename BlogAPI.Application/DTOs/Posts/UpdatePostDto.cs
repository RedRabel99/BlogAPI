namespace BlogAPI.Application.DTOs.Posts;

public class UpdatePostDto
{
    public required string Title { get; set; }
    public required string Excerpt { get; set; }
    public required string Content { get; set; }
    public required List<string> Tags { get; set; } = new List<string>();
}