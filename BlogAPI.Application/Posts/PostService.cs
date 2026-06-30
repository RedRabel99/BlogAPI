using BlogAPI.Application.Posts.Dtos;
using BlogAPI.Application.Common.Querying;
using BlogAPI.Application.Common.Validation;
using BlogAPI.Application.Tags;
using BlogAPI.Application.Common.Email;
using BlogAPI.Application.Common.Persistance;
using BlogAPI.Application.Common.Pagination;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Auth;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Slugify;
using BlogAPI.Application.Common.Errors;

namespace BlogAPI.Application.Posts;

public class PostService : IPostService
{
    private readonly IAppDbContext _appDbContext;
    private readonly IValidator<CreatePostDto> _createPostValidator;
    private readonly IUserContext _userContext;
    private readonly ITagService _tagService;
    private readonly ISlugHelper _slugHelper;
    private readonly IValidator<PostQueryParametersDto> _postQueryParameterDtoValidator;
    private readonly IValidator<UpdatePostDto> _updatePostDtoValidator;
    private readonly IPagedListFactory _pagedListFactory;
    private readonly IEmailQueue _emailQueue;

    public PostService(
        IAppDbContext appDbContext,
        IValidator<CreatePostDto> createPostValidator,
        IUserContext userContext,
        ITagService tagService,
        ISlugHelper slugHelper,
        IValidator<PostQueryParametersDto> postQueryParameterDtoValidator,
        IPagedListFactory pagedListFactory,
        IValidator<UpdatePostDto> updatePostDtoValidator,
        IEmailQueue emailQueue)
    {
        _appDbContext = appDbContext;
        _createPostValidator = createPostValidator;
        _userContext = userContext;
        _tagService = tagService;
        _slugHelper = slugHelper;
        _postQueryParameterDtoValidator = postQueryParameterDtoValidator;
        _pagedListFactory = pagedListFactory;
        _updatePostDtoValidator = updatePostDtoValidator;
        _emailQueue = emailQueue;
    }

    public async Task<Result<PostDto>> CreatePost(CreatePostDto createPostDto)
    {
        var validationResult = await _createPostValidator.ValidateAsync(createPostDto);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure<PostDto>();
        }

        if (!_userContext.IsAuthenticated || string.IsNullOrEmpty(_userContext.UserId))
        {
            return Result<PostDto>.Failure(AuthErrors.UserNotFound);
        }

        var userProfile = await _appDbContext.UserProfiles
            .FirstOrDefaultAsync(up => up.ApplicationUserId == _userContext.UserId);

        if (userProfile is null)
        {
            return Result<PostDto>.Failure(AuthErrors.UserNotFound);
        }

        var tagList = await _tagService.ResolveTagsAsync(createPostDto.Tags);

        var slug = string.IsNullOrEmpty(createPostDto.Slug) ? _slugHelper.GenerateSlug(createPostDto.Title) : createPostDto.Slug;

        var slugTaken = await _appDbContext.Posts
            .AnyAsync(p => p.UserProfileId == userProfile.Id && p.Slug == slug);

        if (slugTaken)
        {
            return Result<PostDto>.Failure(PostErrors.PostAlreadyExists);
        }

        var post = new Post
        {
            Title = createPostDto.Title,
            Slug = slug,
            Excerpt = createPostDto.Excerpt,
            Content = createPostDto.Content,
            UserProfileId = userProfile.Id,
            UserProfile = userProfile,
            Tags = tagList
        };

        _appDbContext.Posts.Add(post);
        await _appDbContext.SaveChangesAsync();

        return Result<PostDto>.Success(post.ToDto());
    }

    public async Task<Result> DeletePost(Guid id)
    {
        if (!_userContext.IsAuthenticated || string.IsNullOrEmpty(_userContext.UserProfileId))
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        
        var post = await _appDbContext.Posts.FirstOrDefaultAsync(p => p.Id == id);

        if (post is null)
        {
            return Result.Failure(PostErrors.NotFound);
        }

        var userProfileId = Guid.Parse(_userContext.UserProfileId);
        
        if (post.UserProfileId != userProfileId)
        {
            return Result.Failure(PostErrors.Forbidden);
        }

        _appDbContext.Posts.Remove(post);
        await _appDbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<PagedList<PostDto>>> GetAllPosts(PostQueryParametersDto queryParametersDto)
    {
        var validationResult = await _postQueryParameterDtoValidator.ValidateAsync(queryParametersDto);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure<PagedList<PostDto>>();
        }

        var postQueryFilter = new PostSearchQueryFilter(queryParametersDto);
        var postQuerySorting = new PostQuerySorting(queryParametersDto.SortingOrder, queryParametersDto.SortColumn);

        var query = _appDbContext.Posts
            .ApplyFiltering(postQueryFilter)
            .ApplySorting(postQuerySorting)
            .Select(PostMappers.ProjectToDto);

        var result = await _pagedListFactory.CreateAsync(query, queryParametersDto.Page, queryParametersDto.PageSize);

        return Result<PagedList<PostDto>>.Success(result);
    }

    public async Task<Result<PostDto>> GetPostById(Guid id)
    {
        var post = await _appDbContext.Posts
            .Include(p => p.Tags)
            .Include(p => p.UserProfile)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post is null)
        {
            return Result<PostDto>.Failure(PostErrors.NotFound);
        }

        return Result<PostDto>.Success(post.ToDto());
    }

    public async Task<Result<PostDto>> UpdatePost(Guid id, UpdatePostDto updatePostDto)
    {
        var validationResult = await _updatePostDtoValidator.ValidateAsync(updatePostDto);

        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure<PostDto>();
        }

        if (!_userContext.IsAuthenticated || string.IsNullOrEmpty(_userContext.UserProfileId))
        {
            return Result<PostDto>.Failure(AuthErrors.UserNotFound);
        }

        var post = await _appDbContext.Posts
            .Include(p => p.Tags)
            .Include(p => p.UserProfile)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post is null)
        {
            return Result<PostDto>.Failure(PostErrors.NotFound);
        }

        var userProfileId = Guid.Parse(_userContext.UserProfileId);

        if (post.UserProfileId != userProfileId)
        {
            return Result<PostDto>.Failure(PostErrors.Forbidden);
        }

        if (updatePostDto.Tags is not null)
        {
            var tagList = await _tagService.ResolveTagsAsync(updatePostDto.Tags);
            post.Tags.Clear();
            foreach (var tag in tagList)
            {
                post.Tags.Add(tag);
            }
        }

        if (!string.IsNullOrEmpty(updatePostDto.Title))
        {
            post.Title = updatePostDto.Title;
        }

        if (!string.IsNullOrEmpty(updatePostDto.Excerpt))
        {
            post.Excerpt = updatePostDto.Excerpt;
        }

        if (!string.IsNullOrEmpty(updatePostDto.Content))
        {
            post.Content = updatePostDto.Content;
        }

        post.UpdatedAt = DateTime.UtcNow;

        await _appDbContext.SaveChangesAsync();

        return Result<PostDto>.Success(post.ToDto());
    }
}
