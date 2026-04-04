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

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetById(Guid id)
    {
        var result = await _userProfileService.GetUserProfileById(id);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpGet]
    public async Task<IResult> GetUserProfiles([FromQuery] UserProfileQueryParametersDto queryParameters)
    {
        var result = await _userProfileService.GetUserProfiles(queryParameters);
        //I'm keeping the result as returned type in case enpoint is expanded to feature filtering
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPatch("{id:guid}")]
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
