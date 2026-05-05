using BlogAPI.Application.DTOs.Comments;
using BlogAPI.Application.Interfaces;
using BlogAPI.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    public CommentsController(
        ICommentService commentService
        )
    {
        _commentService = commentService;
    }

    [HttpGet("post/{id:guid}/comments")]
    public async Task<IResult> GetCommentsByPostId(Guid id, [FromQuery] CommentQueryParametersDto commentQueryParametersDto)
    {
        var result = await _commentService.GetCommentsByPostIdAsync(id);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPost("post/{id:guid}/comments")]
    public async Task<IResult> CreateComment(Guid id, [FromBody] CreateCommentDto commentCreateDto)
    {
        // Implementation for creating a comment goes here
        return TypedResults.CreatedAtRoute();
    }

    [Authorize]
    [HttpPatch("{id:guid}")]
    public async Task<IResult> UpdateComment(Guid id, [FromBody] UpdateCommentDto commentUpdateDto)
    {
        // Implementation for updating a comment goes here
        return TypedResults.Ok();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteComment(Guid id)
    {
        // Implementation for deleting a comment goes here
        return TypedResults.NoContent();
    }
}
