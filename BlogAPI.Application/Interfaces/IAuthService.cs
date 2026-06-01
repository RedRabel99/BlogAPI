using BlogAPI.Application.DTOs.Auth;
using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Interfaces
{
    public interface IAuthService 
    {
        Task<Result> RegisterAsync(RegisterDto registerDto);
        Task<Result> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto);
        Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailDto resendConfirmationEmailDto);
        Task<Result<string>> LoginAsync(LoginDto loginDto);
        Task<Result> ChangeUsernameAsync(ChangeUsernameDto changeUsernameDto);
        Task<Result> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        Task<Result> ChangeEmailAsync(ChangeEmailDto changeEmailDto);
        Task<Result> GenerateChangeEmailTokenAsync(GenerateChangeEmailTokenDto generateChangeEmailTokenDto);
        Task<Result> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<Result> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}
