namespace BlogAPI.Application.Posts.Dtos;

public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public AuthorDto Author { get; set; }
    public string Slug { get; set; }
    public string Excerpt { get; set; }
    public string Content { get; set; }
    public List<string> Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
