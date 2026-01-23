namespace BlogAPI.Application.Shared.UserProfile;

public record UserProfileQuerySorting(string sortingOrder, string? sortColumn)
{
    public SortingOrder SortingOrder { get; set; } = SortingOrderMapper.MapToSortingOrder(sortingOrder);
    public string? SortColumn { get; set; } = sortColumn;
}