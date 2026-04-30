using BlogAPI.Application.DTOs.Posts;
using BlogAPI.Application.Extensions;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.Posts;
using BlogAPI.Application.Mapping;
using FluentValidation;
using Slugify;

namespace BlogAPI.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IValidator<CreatePostDto> _createPostValidator;
    private readonly IUserContext _userContext;
    private readonly ITagService _tagService;
    private readonly ISlugHelper _slugHelper;
    public PostService(
        IPostRepository postRepository,
        IValidator<CreatePostDto> createPostValidator,
        IUserContext userContext,
        ITagService tagService)
    {
        _postRepository = postRepository;
        _createPostValidator = createPostValidator;
        _userContext = userContext;
        _tagService = tagService;
    }

    public async Task<Result<PostDto>> CreatePost(CreatePostDto createPostDto)
    {
        var validationResult = _createPostValidator.Validate(createPostDto);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure<PostDto>();
        }

        if(!_userContext.IsAuthenticated || string.IsNullOrEmpty(_userContext.UserId))
        {
            return Result<PostDto>.Failure(AuthErrors.UserNotFound); //TODO: dodac wlasciwy error
        }

        var tagList = await _tagService.ResolveTagsAsync(createPostDto.Tags);

        var user = await _userContext.GetCurrentUserAsync();
        var userProfileId = Guid.Parse(user.UserProfileId);

        var slug = string.IsNullOrEmpty(createPostDto.Slug) ? _slugHelper.GenerateSlug(createPostDto.Title) : createPostDto.Slug;

        if (await _postRepository.GetPostBySlugAndUser(userProfileId, createPostDto.Slug) is not null)
        {
            return Result<PostDto>.Failure(PostErrors.PostAlreadyExists);
        }
        var post = new Post
        {
            Title = createPostDto.Title,
            Slug = createPostDto.Slug,
            Excerpt = createPostDto.Excerpt,
            Content = createPostDto.Content,
            UserProfileId = userProfileId,
            Tags = tagList
        };

        var createdPost = await _postRepository.CreatePostAsync(post);

        if(createdPost is null)
        {
            return Result<PostDto>.Failure(PostErrors.Internal);
        }

        return Result<PostDto>.Success(createdPost.ToDto());
    }

    public Task<Result> DeletePost(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PagedList<PostDto>>> GetAllPosts()
    {
        throw new NotImplementedException();
    }

    public Task<Result<PostDto>> GetPostById(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PagedList<PostDto>>> GetPostsByCurrentUser()
    {
        throw new NotImplementedException();
    }

    public Task<Result<PagedList<PostDto>>> GetPostsByUsername(string username)
    {
        throw new NotImplementedException();
    }

    public Task<Result<PagedList<PostDto>>> GetPostsByUserProfileId()
    {
        throw new NotImplementedException();
    }

    public Task<Result<PostDto>> UpdatePost(Guid id, UpdatePostDto updatePostDto)
    {
        throw new NotImplementedException();
    }
}
