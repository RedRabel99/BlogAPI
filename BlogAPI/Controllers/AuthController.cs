using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


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
    public async Task<IResult> Register([FromBody]RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        return result.IsSuccess ? Results.Ok(new {Message = "User registered"}) : result.ToProblemDetails();
    }

    [HttpPost("login")]
    public async Task<IResult> Login([FromBody]LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPatch("username")]
    public async Task<IResult> ChangeUsername([FromBody]ChangeUsernameDto changeUsernameDto)
    {
        var result = await _authService.ChangeUsernameAsync(changeUsernameDto);
        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPatch("password")]
    public async Task<IResult> ChangePassword([FromBody]ChangePasswordDto changePasswordDto)
    {
        var result = await _authService.ChangePasswordAsync(changePasswordDto);
        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPost("email/change-token")]
    public async Task<IResult> GenerateChangeEmailToken([FromBody]GenerateChangeEmailTokenDto generateChangeEmailTokenDto)
    {
        var result = await _authService.GenerateChangeEmailTokenAsync(generateChangeEmailTokenDto);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPatch("email")]
    public async Task<IResult> ChangeEmailAsync([FromBody] ChangeEmailDto changeEmailDto)
    {
        var result = await _authService.ChangeEmailAsync(changeEmailDto);
        return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
    }


    [Authorize]
    [HttpGet("test")]
    public async Task<IResult> Test()
    {
        return Results.Ok(new
        {
            result = "it works"
        });
    }
}
