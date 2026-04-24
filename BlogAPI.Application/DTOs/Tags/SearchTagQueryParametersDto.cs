namespace BlogAPI.Application.DTOs.Tags
{
    public class SearchTagQueryParametersDto
    {
        public string? TagName { get; set; }
        public string? SortColumn { get; set; } = "TagName";
        public string SortingOrder { get; set; } = "asc";

        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}