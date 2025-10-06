using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Domain;

public static class AuthErrors
{
    public static readonly Error UserNotFound =
        Error.NotFound("AuthErrors.NotFound","Given user was not found");
    public static readonly Error NoAuthenticatedUser =
        Error.NotFound("AuthErrors.NoAuthenticatedIser", "There is no user authenticated currently");

    public static readonly Error InvalidCredentials =
        Error.Forbidden("AuthErrors.InvalidCredentials", "Given credentials are invalid");
}
