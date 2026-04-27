using BlogAPI.Application.DTOs.Tags;

namespace BlogAPI.Application.DTOs.Posts;

public class CreatePostDto
{
    public string Title { get; set; }
    public string Body { get; set; }
    public List<TagDto> Tags { get; set; }
}
