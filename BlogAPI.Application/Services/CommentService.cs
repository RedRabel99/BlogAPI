using BlogAPI.Application.DTOs.Comments;
using BlogAPI.Application.Extensions;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Mapping;
using BlogAPI.Application.Shared;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.Comments;
using BlogAPI.Domain.Interfaces.Posts;
using BlogAPI.Domain.Interfaces.UserProfiles;
using FluentValidation;

namespace BlogAPI.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IPagedListFactory _pagedListFactory;
    private readonly IValidator<CreateCommentDto> _createCommentValidator;
    private readonly IValidator<UpdateCommentDto> _updateCommentValidator;
    private readonly IValidator<CommentQueryParametersDto> _commentQueryParametersValidator;
    private readonly IUserContext _userContext;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUserProfileRepository userProfileRepository,
        IPagedListFactory pagedListFactory,
        IValidator<CreateCommentDto> createCommentValidator,
        IValidator<UpdateCommentDto> updateCommentValidator,
        IValidator<CommentQueryParametersDto> commentQueryParametersValidator,
        IUserContext userContext)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _userProfileRepository = userProfileRepository;
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

        var post = await _postRepository.GetPostAsync(postId);
        if (post is null)
        {
            return Result<CommentDto>.Failure(PostErrors.NotFound);
        }

        var userProfile = await _userProfileRepository.GetByApplicationUserIdAsync(_userContext.UserId);
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

        var createdComment = await _commentRepository.CreateAsync(comment);

        return Result<CommentDto>.Success(createdComment.ToDto());
    }

    public async Task<Result<CommentDto>> GetCommentByIdAsync(Guid id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
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

        var post = await _postRepository.GetPostAsync(postId);
        if (post is null)
        {
            return Result<PagedList<CommentDto>>.Failure(PostErrors.NotFound);
        }

        var queryFiltering = new CommentQueryFiltering(queryParameters);
        var querySorting = new CommentQuerySorting(queryParameters.SortingOrder, queryParameters.SortColumn);

        var query = _commentRepository.GetQuery()
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

        var comment = await _commentRepository.GetByIdAsync(id);
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

        await _commentRepository.UpdateAsync(comment);

        return Result<CommentDto>.Success(comment.ToDto());
    }

    public async Task<Result> DeleteCommentAsync(Guid id)
    {
        if (!_userContext.IsAuthenticated || string.IsNullOrEmpty(_userContext.UserProfileId))
        {
            return Result.Failure(CommentErrors.Unauthorized);
        }

        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment is null)
        {
            return Result.Failure(CommentErrors.NotFound);
        }

        var userProfileId = Guid.Parse(_userContext.UserProfileId);
        if (comment.UserProfileId != userProfileId)
        {
            return Result.Failure(CommentErrors.Forbidden);
        }

        await _commentRepository.DeleteAsync(comment);

        return Result.Success();
    }
}
