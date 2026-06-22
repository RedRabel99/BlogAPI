using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Domain;

public static class RefreshTokenErrors
{
    public static readonly Error Invalid =
        Error.Unauthorized("RefreshTokenErrors.Invalid", "Refresh token is invalid.");
}
