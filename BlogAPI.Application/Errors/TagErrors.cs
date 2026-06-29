using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Errors;

public static class TagErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Tag.NotFound", "Given tag was not found");
}
