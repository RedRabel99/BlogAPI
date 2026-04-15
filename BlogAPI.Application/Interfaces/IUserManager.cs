using BlogAPI.Application.DTOs.Auth;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Interfaces.Auth;

public interface IUserManager
{
    Task<Result> CreateUserAsync(RegisterDto registerDto);
    Task<Result<IUserInfo>> ValidateUserAsync(string email, string password);
    Task<Result<IUserInfo>> FindByEmailAsync(string email);
    Task<Result<IUserInfo>> FindByIdAsync(string userId);
    Task<Result> ChangeUsernameAsync(string userId, string username);
    Task<Result<string>> GenerateChangeEmailTokenAsync(string userId, string newEmail);
    Task<Result> ChangeEmailAsync(string userId, string newEmail, string token);
    Task<Result> ChangePasswordAsync(string userId, string oldPassword, string newPassword);

}
