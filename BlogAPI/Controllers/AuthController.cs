using BlogAPI.Application.Auth.Dtos;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Auth;
using BlogAPI.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;


namespace BlogAPI.Web.Controllers;
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IResult> Register([FromBody]RegisterDto registerDto, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(registerDto, ct);
        return result.IsSuccess ? TypedResults.Ok(new {Message = "User registered"}) : result.ToProblemDetails();
    }

    [HttpPost("login")]
    public async Task<IResult> Login([FromBody]LoginDto loginDto, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(loginDto, ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IResult> Refresh([FromBody]RefreshRequestDto refreshRequestDto, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(refreshRequestDto, ct);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IResult> Logout([FromBody]LogoutRequestDto logoutRequestDto, CancellationToken ct)
    {
        var result = await _authService.LogoutAsync(logoutRequestDto, ct);
        return result.IsSuccess ? TypedResults.Ok() : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPatch("username")]
    public async Task<IResult> ChangeUsername([FromBody]ChangeUsernameDto changeUsernameDto, CancellationToken ct)
    {
        var result = await _authService.ChangeUsernameAsync(changeUsernameDto, ct);
        return result.IsSuccess ? TypedResults.Ok() : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPatch("password")]
    public async Task<IResult> ChangePassword([FromBody]ChangePasswordDto changePasswordDto, CancellationToken ct)
    {
        var result = await _authService.ChangePasswordAsync(changePasswordDto, ct);
        return result.IsSuccess ? TypedResults.Ok() : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPost("email/change-token")]
    public async Task<IResult> GenerateChangeEmailToken([FromBody]GenerateChangeEmailTokenDto generateChangeEmailTokenDto, CancellationToken ct)
    {
        var result = await _authService.GenerateChangeEmailTokenAsync(generateChangeEmailTokenDto, ct);
        return result.IsSuccess ? TypedResults.Ok() : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPatch("email")]
    public async Task<IResult> ChangeEmailAsync([FromBody] ChangeEmailDto changeEmailDto, CancellationToken ct)
    {
        var result = await _authService.ChangeEmailAsync(changeEmailDto, ct);
        return result.IsSuccess ? TypedResults.Ok() : result.ToProblemDetails();
    }

    [AllowAnonymous]
    [HttpPost("resent-confirmation")]
    [EnableRateLimiting("auth-resend")] //TODO: add rate limiting policy
    public async Task<IResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailDto resendConfirmationEmailDto, CancellationToken ct)
    {
        var result = await _authService.ResendConfirmationEmailAsync(resendConfirmationEmailDto, ct);
        return result.IsSuccess ? TypedResults.Ok() : result.ToProblemDetails();
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto, CancellationToken ct)
    {
        var result = await _authService.ForgotPasswordAsync(forgotPasswordDto, ct);
        return result.IsSuccess ? TypedResults.Ok() : result.ToProblemDetails();
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto, CancellationToken ct)
    {
        var result = await _authService.ResetPasswordAsync(resetPasswordDto, ct);
        return result.IsSuccess ? TypedResults.Ok() : result.ToProblemDetails();
    }

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<IResult> ConfirmEmail([FromBody] ConfirmEmailDto confirmEmailDto, CancellationToken ct)
    {
        var result = await _authService.ConfirmEmailAsync(confirmEmailDto, ct);
        return result.IsSuccess ? TypedResults.Ok() : result.ToProblemDetails();
    }

    [Authorize]
    [HttpGet("test")]
    public async Task<IResult> Test()
    {
        return TypedResults.Ok(new
        {
            result = "it works"
        });
    }
}
