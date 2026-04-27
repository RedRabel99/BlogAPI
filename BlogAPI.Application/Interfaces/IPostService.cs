using BlogAPI.Application.DTOs.Posts;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Interfaces;

public interface IPostService
{
    Task<Result> CreatePost(CreatePostDto createPostDto);
    Task<Result<PostDto>> GetPostById(Guid id);
    Task<Result<PagedList<PostDto>>> GetAllPosts();
    Task<Result<PagedList<PostDto>>> GetPostsByUserProfileId();
    Task<Result<PagedList<PostDto>>> GetPostsByUsername(string username);
    Task<Result<PagedList<PostDto>>> GetPostsByCurrentUser();
    Task<Result<PostDto>> UpdatePost(Guid id);
    Task<Result> DeletePost(Guid id);
}
