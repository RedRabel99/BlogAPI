using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Domain;

public static class TagErrors
{
    public static readonly Error TagNotFound =
        Error.NotFound("TagErrors.NotFound", "Given tag was not found");
}
