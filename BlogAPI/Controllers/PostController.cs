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

        [Authorize]
        [HttpPost]
        public async Task<IResult> CreatePost([FromBody]CreatePostDto createPostDto)
        {
            var result = await _postService.CreatePost(createPostDto);
            return result.IsSuccess ? Results.Created(result.Value) : result.ToProblemDetails();
        }

        [HttpGet("{id:guid}")]
        public async Task<IResult> GetPostById(Guid id)
        {
            var result = await _postService.GetPostById(id);
            return result.ISuccess
        }
    }
}
