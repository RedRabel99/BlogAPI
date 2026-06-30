using BlogAPI.Application.Comments.Dtos;
using BlogAPI.Application.Common.Pagination;
using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Comments;

public interface ICommentService
{
    Task<Result<CommentDto>> GetCommentByIdAsync(Guid id);
    Task<Result<PagedList<CommentDto>>> GetCommentsByPostIdAsync(Guid id, CommentQueryParametersDto queryParameters);
    Task<Result<CommentDto>> CreateCommentAsync(Guid postId, CreateCommentDto createCommentDto);
    Task<Result<CommentDto>> UpdateCommentAsync(Guid id, UpdateCommentDto updateCommentDto);
    Task<Result> DeleteCommentAsync(Guid id);
}
