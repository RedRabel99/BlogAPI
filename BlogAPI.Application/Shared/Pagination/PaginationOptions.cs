namespace BlogAPI.Application.Shared.Pagination;

public sealed class PaginationOptions
{   
    public int DefaultPage { get; init; }
    public int DefaultPageSize { get; init; }
    public int MaxPageSize { get; init; }
    public int MinPageSize { get; init; }
}
