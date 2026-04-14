using BlogAPI.Application.DTOs;
using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Interfaces
{
    public interface IAuthService 
    {
        Task<Result> RegisterAsync(RegisterDto registerDto);
        Task<Result<string>> LoginAsync(LoginDto loginDto);
        Task<Result> ChangeUsernameAsync(ChangeUsernameDto changeUsernameDto);
        Task<Result> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        Task<Result> ChangeEmailAsync(ChangeEmailDto changeEmailDto);
        Task<Result<ChangeEmailTokenResponseDto>> GenerateChangeEmailTokenAsync(GenerateChangeEmailTokenDto generateChangeEmailTokenDto);
    }
}
