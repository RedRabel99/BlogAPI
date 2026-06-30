namespace BlogAPI.Application.Posts.Dtos;

public class UpdatePostDto
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Excerpt { get; set; }
    public string? Content { get; set; }
    public List<string>? Tags { get; set; }
}