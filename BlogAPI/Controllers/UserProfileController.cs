using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UserProfileController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IResult> GetCurrentUserProfile()
    {
        var result = await _userProfileService.GetCurrentUserProfileAsync();
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpGet("{username}")]
    public async Task<IResult> GetByUsername(string username)
    {
        var result = await _userProfileService.GetUserProfileByUsername(username);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpGet]
    public async Task<IResult> GetUserProfiles([FromQuery] UserProfileQueryParametersDto queryParameters)
    {
        var result = await _userProfileService.GetUserProfiles(queryParameters);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IResult> Update(Guid id, [FromBody] UpdateUserProfileDto userProfile)
    {
        var result = await _userProfileService.UpdateUserProfile(id, userProfile);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IResult> Delete(Guid id)
    {
        var result = await _userProfileService.DeleteUserProfileById(id);
        return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
    }
}
