using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Domain;

public static class AuthErrors
{
    public static readonly Error UserNotFound =
        new Error("Given user was not found");

    public static readonly Error InvalidCredentials =
        new Error("Invalid credentials");
}
