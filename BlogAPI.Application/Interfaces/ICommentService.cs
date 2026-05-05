using BlogAPI.Application.DTOs.Comments;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Interfaces;

public interface ICommentService
{
    Task<Result<CommentDto>> GetCommentById();
    Task<Result<PagedList<CommentDto>>> GetCommentsByPostIdAsync(Guid id, CommentQueryParametersDto commentQueryParametersDto);
    Task<Result<CommentDto>> CreateCommentAsync(Guid postId, CreateCommentDto createCommentDto);
    Task<Result<CommentDto>> UpdateCommentAsync(Guid id, UpdateCommentDto updateCommentDto);
}
