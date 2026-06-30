namespace BlogAPI.Application.Tags.Dtos;

public record TagDto
{
    public string TagName { get; set; }
    public string Slug { get; set; }
}