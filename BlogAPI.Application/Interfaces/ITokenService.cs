using BlogAPI.Application.DTOs.Auth;
using BlogAPI.Domain.Interfaces.Auth;

namespace BlogAPI.Application.Interfaces.Auth;

public interface ITokenService
{
    Task<AccessTokenResultDto> GenerateAccessTokenAsync(IUserInfo user);
}
