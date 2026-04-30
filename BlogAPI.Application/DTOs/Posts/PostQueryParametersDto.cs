namespace BlogAPI.Application.DTOs.Posts;

public class PostQueryParametersDto
{
    public string? Title { get; set; }
    public string? Excerpt { get; set; }
    public string? UserName { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public List<string> Tags { get; set; }=  new List<string>();
    public string? SortColumn { get; set; } = "Username";
    public string SortingOrder { get; set; } = "asc";

    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
