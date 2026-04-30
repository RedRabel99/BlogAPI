using BlogAPI.Application.DTOs.Tags;

namespace BlogAPI.Application.DTOs.Posts;

public class CreatePostDto
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
}
