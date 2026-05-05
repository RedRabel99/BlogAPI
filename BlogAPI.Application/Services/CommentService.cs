using BlogAPI.Application.DTOs.Comments;
using BlogAPI.Application.Extensions;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.Comments;
using BlogAPI.Domain.Interfaces.UserProfiles;
using FluentValidation;

namespace BlogAPI.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly PagedListFactory _pagedListFactory;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IValidator<CreateCommentDto> _createCommentValidator;
    private readonly IValidator<UpdateCommentDto> _updateCommentValidator;
    private readonly IValidator<CommentQueryParametersDto> _commentQueryParametersValidator;
    private readonly IUserContext _userContext;

    public CommentService(
        ICommentRepository commentRepository,
        IUserProfileRepository userProfileRepository,
        PagedListFactory pagedListFactory,
        IValidator<CreateCommentDto> createCommentValidator,
        IValidator<UpdateCommentDto> updateCommentValidator,
        IValidator<CommentQueryParametersDto> commentQueryParametersValidator,
        IUserContext userContext)
    {
        _commentRepository = commentRepository;
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

        if(!_userContext.IsAuthenticated && string.IsNullOrEmpty(_userContext.UserId))
        {
            return Result<CommentDto>.Failure(CommentErrors.Unautorized);
        }

        var userProfile = await _userProfileRepository.GetByApplicationUserIdAsync(_userContext.UserId);

        if(userProfile is null)
        {
            return Result<CommentDto>.Failure(UserProfileErrors.NotFound);
        }

        var comment = new Comment
        {
            Content = createCommentDto.Content,
            UserProfileId = userProfile.Id,
            UserProfile = userProfile,
            PostId = postId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdComment = await _commentRepository.CreateAsync(comment);

        return Result<CommentDto>.Success(createComment.ToDto());
    }

    public Task<Result<CommentDto>> GetCommentById()
    {
        throw new NotImplementedException();
    }

    public Task<Result<PagedList<CommentDto>>> GetCommentsByPostIdAsync(Guid id, CommentQueryParametersDto commentQueryParametersDto)
    {
        throw new NotImplementedException();
    }

    public Task<Result<CommentDto>> UpdateCommentAsync(Guid id, UpdateCommentDto updateCommentDto)
    {
        throw new NotImplementedException();
    }
}
