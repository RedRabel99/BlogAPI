namespace BlogAPI.Application.DTOs.Tags;

public record TagDto
{
    public string TagName { get; set; }
    public string Slug { get; set; }
}