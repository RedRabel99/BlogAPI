using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Errors;

public static class AuthErrors
{
    public static readonly Error UserNotFound =
        Error.NotFound("Auth.NotFound", "Given user was not found");
    public static readonly Error NoAuthenticatedUser =
        Error.NotFound("Auth.NoAuthenticatedUser", "There is no user authenticated currently");
    public static readonly Error PasswordMismatch =
        Error.Validation("Auth.PasswordMismatch", "Given password is not a valid password");
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "Given credentials are invalid");
    public static readonly Error AuthFailure =
        Error.Internal("Auth.AuthFailure", "Something went wrong while authenticating");
    public static readonly Error UserAlreadyExists =
        Error.Conflict("Auth.UserAlreadyExists", "User with given username already exists");
    public static readonly Error UserWithEmailAlreadyExists =
        Error.Conflict("Auth.UserWithEmailAlreadyExists", "User with given email already exists");
    public static readonly Error Internal =
        Error.Internal("Auth.Internal", "Something went wrong");
    public static readonly Error InvalidToken =
        Error.Validation("Auth.InvalidToken", "Given token is invalid");
}
