namespace BlogAPI.Domain.Interfaces.Auth;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(IUserInfo user);
}
