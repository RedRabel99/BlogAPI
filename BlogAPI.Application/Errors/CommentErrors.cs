using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Errors;

public static class CommentErrors
{
    public static Error Unauthorized =
        Error.Unauthorized("Comment.Unauthorized", "User must be authenticated to perform this action.");
    public static Error Forbidden =
        Error.Forbidden("Comment.Forbidden", "User is not allowed to perform this action on the comment.");
    public static Error NotFound =
        Error.NotFound("Comment.NotFound", "The specified comment was not found.");
    public static Error Internal =
        Error.Internal("Comment.Internal", "An unexpected error occurred while processing the comment.");
}
