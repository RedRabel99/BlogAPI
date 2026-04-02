using BlogAPI.Application;
using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public async Task<IResult> Register(RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        return result.IsSuccess ? Results.Created() : result.ToProblemDetails();
    }

    [HttpPost("login")]
    public async Task<IResult> Login(LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
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
