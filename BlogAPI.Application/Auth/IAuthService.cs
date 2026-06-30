using BlogAPI.Application.Auth.Dtos;
using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Auth
{
    public interface IAuthService 
    {
        Task<Result> RegisterAsync(RegisterDto registerDto, CancellationToken ct = default);
        Task<Result> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto, CancellationToken ct = default);
        Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailDto resendConfirmationEmailDto, CancellationToken ct = default);
        Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken ct = default);
        Task<Result> ChangeUsernameAsync(ChangeUsernameDto changeUsernameDto, CancellationToken ct = default);
        Task<Result> ChangePasswordAsync(ChangePasswordDto changePasswordDto, CancellationToken ct = default);
        Task<Result> ChangeEmailAsync(ChangeEmailDto changeEmailDto, CancellationToken ct = default);
        Task<Result> GenerateChangeEmailTokenAsync(GenerateChangeEmailTokenDto generateChangeEmailTokenDto, CancellationToken ct = default);
        Task<Result> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto, CancellationToken ct = default);
        Task<Result> ResetPasswordAsync(ResetPasswordDto resetPasswordDto, CancellationToken ct = default);
        Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshRequestDto refreshRequestDto, CancellationToken ct = default);
        Task<Result> LogoutAsync(LogoutRequestDto logoutRequestDto, CancellationToken ct = default);
    }
}
