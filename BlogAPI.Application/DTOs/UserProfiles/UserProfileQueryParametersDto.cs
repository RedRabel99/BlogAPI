using BlogAPI.Application.Shared;

namespace BlogAPI.Application.DTOs.UserProfiles;

public class UserProfileQueryParametersDto
{
    public string? UserName { get; set; } 
    public string? DisplayName{ get; set; }
    public string? SortColumn { get; set; } = "Username";
    public string SortingOrder { get; set; } = "asc";

    public int? Page { get; set; }
    public int? PageSize { get; set; }
}