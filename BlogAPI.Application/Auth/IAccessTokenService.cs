using BlogAPI.Domain.Interfaces.Auth;

namespace BlogAPI.Application.Auth;

public record AccessTokenResult(string AccessToken, int ExpiresInSeconds);

public interface IAccessTokenService
{
    Task<AccessTokenResult> GenerateAccessTokenAsync(IUserInfo user);
}
