using BlogAPI.Application;
using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
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
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        var registerResult = await _authService.RegisterAsync(registerDto);

        if (registerResult.IsError is true)
        {
            return BadRequest(new { errors = new { registerResult.Error } });
        }

        return Ok(new
        {
            //TODO:later, change the registrations endpoint not to generate and return the token
            token = registerResult.Value,
            message = "Registration succesful"
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);

        if (result.IsError is true)
        {
            return Unauthorized(new { errors = new { result.Error } });
        }

        return Ok(new
        {
            token = result.Value,
            message = "Login successful"
        });
    }

    [Authorize]
    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        return Ok(new
        {
            result = "it works"
        });
    }
}
