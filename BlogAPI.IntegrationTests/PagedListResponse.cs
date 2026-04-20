namespace BlogAPI.IntegrationTests;

public record PagedListResponse<T>(List<T> Items, int Page, int PageSize, int TotalCount);