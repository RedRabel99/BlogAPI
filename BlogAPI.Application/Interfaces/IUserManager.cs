using BlogAPI.Application.DTOs;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Interfaces.Auth;

public interface IUserManager
{
    Task<Result> CreateUserAsync(RegisterDto registerDto);
    Task<Result<IUserInfo>> ValidateUserAsync(string email, string password);
    Task<Result<IUserInfo>> FindByEmailAsync(string email);
    Task<Result<IUserInfo>> FindByIdAsync(string userId);
    Task RemoveByIdAsync(string userId);
}
