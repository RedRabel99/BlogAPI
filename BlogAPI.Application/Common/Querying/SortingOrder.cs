using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BlogAPI.Application.Common.Querying;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortingOrder
{
    [EnumMember(Value = "asc")]
    Ascending,
    [EnumMember(Value = "dsc")]
    Descending,
}
