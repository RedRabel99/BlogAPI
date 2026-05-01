namespace BlogAPI.Application.DTOs.Posts;

public class PostQueryParametersDto
{
    public string? Title { get; set; }
    public List<string> Usernames { get; set; } = [];
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public List<string> Tags { get; set; }=  [];
    public string? SortColumn { get; set; } = "Username";
    public string SortingOrder { get; set; } = "asc";

    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
