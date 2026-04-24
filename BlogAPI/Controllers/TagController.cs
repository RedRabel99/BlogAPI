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
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpGet("{slug}")]
    public async Task<IResult> GetTagBySlug(string slug)
    {
        var result = await _tagService.GetTagBySlugAsync(slug);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpGet("{slug}/posts")]
    public async Task<IResult> GetPostsByTagSlug(string slug, [FromQuery] SearchTagQueryParametersDto queryParameters)
    {
        var result = await _tagService.GetTagsByPostIdAsync(Guid.Parse(slug));
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
    }
}
