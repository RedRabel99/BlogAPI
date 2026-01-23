using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BlogAPI.Application.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortingOrder
{
    [EnumMember(Value = "asc")]
    Ascending,
    [EnumMember(Value = "dsc")]
    Descending,
}

public static class SortingOrderMapper
{
    public static SortingOrder MapToSortingOrder(string? sortingOrder)
    {
        return string.IsNullOrEmpty(sortingOrder) && sortingOrder == "asc" ?
            SortingOrder.Ascending : SortingOrder.Descending;
    }
}