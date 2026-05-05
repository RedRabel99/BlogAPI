using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Domain;

public static class CommentErrors
{
    public static Error Unautorized =
        Error.Forbidden("Comment.Unauthorized", "User must be authenticated to perform this action.");
    public static Error NotFound =
        Error.NotFound("Comment.NotFound", "The specified comment was not found.");
    public static Error Internal = 
        Error.Internal("Comment.InternalError", "An unexpected error occurred while processing the comment.");
}
