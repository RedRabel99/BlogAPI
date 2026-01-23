using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Interfaces.Auth;

public interface IUserManager
{
    Task<Result<IUserInfo>> CreateUserAsync(string email, string password);
    Task<Result<IUserInfo>> ValidateUserAsync(string email, string password);
    Task<Result<IUserInfo>> FindByEmailAsync(string email);
    Task<Result<IUserInfo>> FindByIdAsync(string userId);
}
