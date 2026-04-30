using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Domain;

public static class PostErrors
{
    public static Error PostAlreadyExists =
        Error.Conflict("Post.PostAlreadyExists", "Post with given slug already exists for this user");

    public static Error Internal =
        Error.Internal("Post.Internal", "An internal error occurred while processing the post");
}
