using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Common.Errors;

public static class RefreshTokenErrors
{
    public static readonly Error Invalid =
        Error.Unauthorized("RefreshToken.Invalid", "Refresh token is invalid.");
}
