    using BlogAPI.Domain.Abstractions;

    namespace BlogAPI.Domain;

    public static class UserProfileErrors
    {
        public static Error NotFound = 
            Error.NotFound("UserProfile.NotFound", "Given user wasn not found");
        public static Error Forbidden = 
            Error.Forbidden("UserProfile.Forbidden", "You are not authorized to perform this action");
        public static Error Internal =
            Error.Internal("UserProfile.Internal", "Something went wrong");
    }
