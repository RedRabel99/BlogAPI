using BlogAPI.Application.Shared;

namespace BlogAPI.Application.DTOs;

public class UserProfileQueryParametersDto
{
    public string? UserName { get; set; } 
    public string? DisplayName{ get; set; }
    public string? SortColumn { get; set; } = "UserName";
    public SortingOrder SortingOrder { get; set; } = SortingOrder.Ascending;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}