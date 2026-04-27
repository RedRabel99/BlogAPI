namespace BlogAPI.Application.DTOs.Posts;

public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public Guid UserProfileId { get; set; }
}
