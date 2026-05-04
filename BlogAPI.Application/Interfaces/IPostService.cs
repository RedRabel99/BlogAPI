using BlogAPI.Application.DTOs.Posts;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Interfaces;

public interface IPostService
{
    Task<Result<PostDto>> CreatePost(CreatePostDto createPostDto);
    Task<Result<PostDto>> GetPostById(Guid id);
    Task<Result<PagedList<PostDto>>> GetAllPosts(PostQueryParametersDto queryParametersDto);
    Task<Result<PostDto>> UpdatePost(Guid id, UpdatePostDto updatePostDto);
    Task<Result> DeletePost(Guid id);
}
