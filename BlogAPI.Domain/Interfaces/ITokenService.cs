namespace BlogAPI.Domain.Interfaces;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(IUserInfo user);
}
