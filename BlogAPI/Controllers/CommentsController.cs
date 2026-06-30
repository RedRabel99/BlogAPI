using BlogAPI.Application.Comments.Dtos;
using BlogAPI.Application.Comments;
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

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("/posts/{postId:guid}/comments")]
    public async Task<IResult> GetCommentsByPostId(Guid postId, [FromQuery] CommentQueryParametersDto queryParameters)
    {
        var result = await _commentService.GetCommentsByPostIdAsync(postId, queryParameters);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpGet("{id:guid}", Name = "GetCommentById")]
    public async Task<IResult> GetCommentById(Guid id)
    {
        var result = await _commentService.GetCommentByIdAsync(id);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPost("/posts/{postId:guid}/comments")]
    public async Task<IResult> CreateComment(Guid postId, [FromBody] CreateCommentDto createCommentDto)
    {
        var result = await _commentService.CreateCommentAsync(postId, createCommentDto);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetCommentById", new { id = result.Value!.Id })
            : result.ToProblemDetails();
    }

    [Authorize]
    [HttpPatch("{id:guid}")]
    public async Task<IResult> UpdateComment(Guid id, [FromBody] UpdateCommentDto updateCommentDto)
    {
        var result = await _commentService.UpdateCommentAsync(id, updateCommentDto);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteComment(Guid id)
    {
        var result = await _commentService.DeleteCommentAsync(id);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }
}
