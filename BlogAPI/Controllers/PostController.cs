using BlogAPI.Application.DTOs.Posts;
using BlogAPI.Application.Interfaces;
using BlogAPI.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogAPI.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IResult> CreatePost([FromBody] CreatePostDto createPostDto)
        {
            var result = await _postService.CreatePost(createPostDto);
            return result.IsSuccess ? Results.CreatedAtRoute($"{result.Value.Id}", result.Value) : result.ToProblemDetails();
        }

        [HttpGet("{id:guid}")]
        public async Task<IResult> GetPostById(Guid id)
        {
            var result = await _postService.GetPostById(id);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
        }

        [HttpGet]
        public async Task<IResult> GetPostList([FromQuery]PostQueryParametersDto queryParametersDto)
        {
            var result = await _postService.GetAllPosts();
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
        }

        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IResult> UpdatePost(Guid id, [FromBody] UpdatePostDto updatePostDto)
        {
            var result = await _postService.UpdatePost(id, updatePostDto);
            return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
        }

        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IResult> DeletePost(Guid id)
        {
            var result = await _postService.DeletePost(id);
            return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
        }
    }
}
