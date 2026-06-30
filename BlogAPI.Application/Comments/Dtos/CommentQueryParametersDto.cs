namespace BlogAPI.Application.Comments.Dtos;

public class CommentQueryParametersDto
{
    public string? Author { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public string? SortColumn { get; set; } = "createdat";
    public string SortingOrder { get; set; } = "desc";

    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
