using BlogAPI.Application.DTOs.Tags;
using BlogAPI.Application.Interfaces;
using BlogAPI.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<IResult> GetTags([FromQuery] SearchTagQueryParametersDto queryParameters)
    {
        var result = await _tagService.GetTagsAsync(queryParameters);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpGet("{tagName}")]
    public async Task<IResult> GetTagByName(string tagName)
    {
        var result = await _tagService.GetTagByNameAsync(tagName);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpGet("from-post/{id:guid}")]
    public async Task<IResult> GetTagsByPostId(Guid id, [FromQuery] SearchTagQueryParametersDto queryParameters)
    {
        var result = await _tagService.GetTagsByPostIdAsync(id, queryParameters);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }
}
