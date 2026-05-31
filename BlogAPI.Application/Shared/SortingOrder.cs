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
