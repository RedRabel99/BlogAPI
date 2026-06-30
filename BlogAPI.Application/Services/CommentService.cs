using BlogAPI.Application.DTOs.Comments;
using BlogAPI.Application.Common.Errors;
using BlogAPI.Application.Common.Querying;
using BlogAPI.Application.Common.Validation;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Common.Persistance;
using BlogAPI.Application.Mapping;
using BlogAPI.Application.Shared;
using BlogAPI.Application.Common.Pagination;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Auth;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Application.Services;

public class CommentService : ICommentService
{
    private readonly IAppDbContext _appDbContext;
    private readonly IPagedListFactory _pagedListFactory;
    private readonly IValidator<CreateCommentDto> _createCommentValidator;
    private readonly IValidator<UpdateCommentDto> _updateCommentValidator;
    private readonly IValidator<CommentQueryParametersDto> _commentQueryParametersValidator;
    private readonly IUserContext _userContext;

    public CommentService(
        IAppDbContext appDbContext,
        IPagedListFactory pagedListFactory,
        IValidator<CreateCommentDto> createCommentValidator,
        IValidator<UpdateCommentDto> updateCommentValidator,
        IValidator<CommentQueryParametersDto> commentQueryParametersValidator,
        IUserContext userContext)
    {
        _appDbContext = appDbContext;
        _pagedListFactory = pagedListFactory;
        _createCommentValidator = createCommentValidator;
        _updateCommentValidator = updateCommentValidator;
        _commentQueryParametersValidator = commentQueryParametersValidator;
        _userContext = userContext;
    }

    public async Task<Result<CommentDto>> CreateCommentAsync(Guid postId, CreateCommentDto createCommentDto)
    {
        var validationResult = await _createCommentValidator.ValidateAsync(createCommentDto);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure<CommentDto>();
        }

        if (!_userContext.IsAuthenticated || string.IsNullOrEmpty(_userContext.UserId))
        {
            return Result<CommentDto>.Failure(CommentErrors.Unauthorized);
        }

        var postExists = await _appDbContext.Posts.AnyAsync(p => p.Id == postId);
        
        if (!postExists)
        {
            return Result<CommentDto>.Failure(PostErrors.NotFound);
        }

        var userProfile = await _appDbContext.UserProfiles
            .FirstOrDefaultAsync(up => up.ApplicationUserId == _userContext.UserId);

        if (userProfile is null)
        {
            return Result<CommentDto>.Failure(UserProfileErrors.NotFound);
        }

        var now = DateTime.UtcNow;
        var comment = new Comment
        {
            Content = createCommentDto.Content,
            UserProfileId = userProfile.Id,
            UserProfile = userProfile,
            PostId = postId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _appDbContext.Comments.Add(comment);
        await _appDbContext.SaveChangesAsync();

        return Result<CommentDto>.Success(comment.ToDto());
    }

    public async Task<Result<CommentDto>> GetCommentByIdAsync(Guid id)
    {
        var comment = await _appDbContext.Comments
            .Include(c => c.UserProfile)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment is null)
        {
            return Result<CommentDto>.Failure(CommentErrors.NotFound);
        }

        return Result<CommentDto>.Success(comment.ToDto());
    }

    public async Task<Result<PagedList<CommentDto>>> GetCommentsByPostIdAsync(Guid postId, CommentQueryParametersDto queryParameters)
    {
        var validationResult = await _commentQueryParametersValidator.ValidateAsync(queryParameters);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure<PagedList<CommentDto>>();
        }

        var postExists = await _appDbContext.Posts.AnyAsync(p => p.Id == postId);
        
        if (!postExists)
        {
            return Result<PagedList<CommentDto>>.Failure(PostErrors.NotFound);
        }

        var queryFiltering = new CommentQueryFiltering(queryParameters);
        var querySorting = new CommentQuerySorting(queryParameters.SortingOrder, queryParameters.SortColumn);

        var query = _appDbContext.Comments
            .Where(c => c.PostId == postId)
            .ApplyFiltering(queryFiltering)
            .ApplySorting(querySorting)
            .Select(CommentMappers.ProjectToDto);

        var result = await _pagedListFactory.CreateAsync(query, queryParameters.Page, queryParameters.PageSize);

        return Result<PagedList<CommentDto>>.Success(result);
    }

    public async Task<Result<CommentDto>> UpdateCommentAsync(Guid id, UpdateCommentDto updateCommentDto)
    {
        var validationResult = await _updateCommentValidator.ValidateAsync(updateCommentDto);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure<CommentDto>();
        }

        if (!_userContext.IsAuthenticated || string.IsNullOrEmpty(_userContext.UserProfileId))
        {
            return Result<CommentDto>.Failure(CommentErrors.Unauthorized);
        }

        var comment = await _appDbContext.Comments
            .Include(c => c.UserProfile)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment is null)
        {
            return Result<CommentDto>.Failure(CommentErrors.NotFound);
        }

        var userProfileId = Guid.Parse(_userContext.UserProfileId);
        if (comment.UserProfileId != userProfileId)
        {
            return Result<CommentDto>.Failure(CommentErrors.Forbidden);
        }

        comment.Content = updateCommentDto.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        
        await _appDbContext.SaveChangesAsync();

        return Result<CommentDto>.Success(comment.ToDto());
    }

    public async Task<Result> DeleteCommentAsync(Guid id)
    {
        if (!_userContext.IsAuthenticated || string.IsNullOrEmpty(_userContext.UserProfileId))
        {
            return Result.Failure(CommentErrors.Unauthorized);
        }

        var comment = await _appDbContext.Comments.FirstOrDefaultAsync(c => c.Id == id);

        if (comment is null)
        {
            return Result.Failure(CommentErrors.NotFound);
        }

        var userProfileId = Guid.Parse(_userContext.UserProfileId);
        if (comment.UserProfileId != userProfileId)
        {
            return Result.Failure(CommentErrors.Forbidden);
        }

        _appDbContext.Comments.Remove(comment);
        await _appDbContext.SaveChangesAsync();

        return Result.Success();
    }
}
