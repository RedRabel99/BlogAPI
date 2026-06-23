using BlogAPI.Domain.Interfaces.Auth;

namespace BlogAPI.Application.Interfaces.Auth;

public record AccessTokenResult(string AccessToken, int ExpiresInSeconds);

public interface IAccessTokenService
{
    Task<AccessTokenResult> GenerateAccessTokenAsync(IUserInfo user);
}
