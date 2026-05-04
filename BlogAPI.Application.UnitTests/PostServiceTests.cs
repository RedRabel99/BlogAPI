using BlogAPI.Application.DTOs.Posts;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Services;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.Posts;
using BlogAPI.Domain.Interfaces.UserProfiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using MockQueryable;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Slugify;

namespace Tests.Application;

public class PostServiceTests
{
    private readonly IPostRepository _postRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IValidator<CreatePostDto> _createPostValidator;
    private readonly IUserContext _userContext;
    private readonly ITagService _tagService;
    private readonly ISlugHelper _slugHelper;
    private readonly IValidator<PostQueryParametersDto> _postQueryParameterDtoValidator;
    private readonly IValidator<UpdatePostDto> _updatePostDtoValidator;
    private readonly IPagedListFactory _pagedListFactory;
    private readonly PostService _sut;

    public PostServiceTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _userProfileRepository = Substitute.For<IUserProfileRepository>();
        _createPostValidator = Substitute.For<IValidator<CreatePostDto>>();
        _userContext = Substitute.For<IUserContext>();
        _tagService = Substitute.For<ITagService>();
        _slugHelper = Substitute.For<ISlugHelper>();
        _postQueryParameterDtoValidator = Substitute.For<IValidator<PostQueryParametersDto>>();
        _updatePostDtoValidator = Substitute.For<IValidator<UpdatePostDto>>();
        _pagedListFactory = new PagedListFactory(Options.Create(new PaginationOptions { DefaultPage = 1, DefaultPageSize = 10 }));

        _sut = new PostService(
            _postRepository,
            _createPostValidator,
            _userContext,
            _tagService,
            _slugHelper,
            _postQueryParameterDtoValidator,
            _pagedListFactory,
            _updatePostDtoValidator,
            _userProfileRepository);
    }

    [Fact]
    public async Task CreatePost_WithEmptyTitle_ReturnsValidationFailure()
    {
        // Arrange
        var createPostDto = new CreatePostDto { Title = string.Empty, Content = "Content of post"};
        var failures = new[] { new ValidationFailure("Title", "Title cant be empty") };
        _createPostValidator.ValidateAsync(Arg.Any<CreatePostDto>()).Returns(new ValidationResult(failures));

        // Act
        var result = await _sut.CreatePost(createPostDto);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.NotEmpty(result.SubErrors);
    }

    [Fact]
    public async Task CreatePost_WhenUserNotAuthenticated_ReturnsUserNotFound()
    {
        // Arrange
        _createPostValidator.ValidateAsync(Arg.Any<CreatePostDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(false);
        _userContext.UserProfileId.Returns((string?)null);

        // Act
        var result = await _sut.CreatePost(new CreatePostDto());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.UserNotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task CreatePost_WhenPostWithGivenSlugExists_ReturnsPostAlreadyExists()
    {
        // Arrange
        var userProfile = new UserProfile { Id = Guid.NewGuid(), Username = "username", ApplicationUserId = "user-id" };
        var createPostDto = new CreatePostDto { Title = "Title", Slug = "title" };
        _createPostValidator.ValidateAsync(createPostDto).Returns(new ValidationResult() );
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns(userProfile.ApplicationUserId);
        _tagService.ResolveTagsAsync(Arg.Any<List<string>>()).Returns(new List<Tag>());
        _postRepository.GetPostBySlugAndUser(Arg.Any<Guid>(), Arg.Any<string>()).Returns(new Post());
        _userProfileRepository.GetByApplicationUserIdAsync(userProfile.ApplicationUserId).Returns(userProfile);

        // Act
        var result = await _sut.CreatePost(createPostDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PostErrors.PostAlreadyExists.Code, result.Error.Code);
    }

    [Fact]
    public async Task CreatePost_WithValidData_ReturnsPostDto()
    {
        // Arrange
        var userProfile = new UserProfile { Id = Guid.NewGuid(), Username = "username", ApplicationUserId = "user-id" };
        var createPostDto = new CreatePostDto
        {
            Title = "Title",
            Excerpt = "Summary",
            Content = "Content of post",
            Tags = new() { "tag" }
        };
        var slug = createPostDto.Title.ToLower();
        _createPostValidator.ValidateAsync(Arg.Any<CreatePostDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns(userProfile.ApplicationUserId);
        _tagService.ResolveTagsAsync(createPostDto.Tags).Returns([new Tag { TagName = "tag" }]);
        _slugHelper.GenerateSlug(createPostDto.Title).Returns(slug);
        _userProfileRepository.GetByApplicationUserIdAsync(userProfile.ApplicationUserId).Returns(userProfile);

        var created = new Post
        {
            Id = Guid.NewGuid(),
            Title = createPostDto.Title,
            Slug = slug,
            Excerpt = createPostDto.Excerpt,
            Content = createPostDto.Content,
            UserProfile = userProfile,
            UserProfileId = Guid.NewGuid(),
        };

        _postRepository.GetPostBySlugAndUser(userProfile.Id, slug).ReturnsNull();
        _postRepository.CreatePostAsync(Arg.Is<Post>(p =>
            p.Title == createPostDto.Title &&
            p.Slug == slug &&
            p.Excerpt == createPostDto.Excerpt &&
            p.Content == createPostDto.Content
        )).Returns(created);

        // Act
        var result = await _sut.CreatePost(createPostDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(created.Title, result.Value.Title);
        Assert.Equal(created.Slug, result.Value.Slug);
        Assert.Equal(created.Excerpt, result.Value.Excerpt);
        Assert.Equal(created.Content, result.Value.Content);
        Assert.Equal(created.UserProfile.Username, result.Value.Author.Username);
    }

    [Fact]
    public async Task DeletePost_WhenNotAuthenticated_ReturnsUserNotFound()
    {
        // Arrange
        _userContext.IsAuthenticated.Returns(false);
        _userContext.UserProfileId.Returns((string?)null);

        // Act
        var result = await _sut.DeletePost(Guid.NewGuid());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.UserNotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task DeletePost_WhenPostDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserProfileId.Returns(Guid.NewGuid().ToString());
        _postRepository.GetPostAsync(Arg.Any<Guid>()).ReturnsNull();

        // Act
        var result = await _sut.DeletePost(Guid.NewGuid());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PostErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task DeletePost_WhenIsNotAuthor_ReturnsForbidden()
    {
        // Arrange
        
        var post = new Post { Id = Guid.NewGuid(), UserProfileId = Guid.NewGuid() };
        _postRepository.GetPostAsync(post.Id).Returns(post);
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserProfileId.Returns(Guid.NewGuid().ToString());
        

        // Act
        var result = await _sut.DeletePost(post.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PostErrors.Forbidden.Code, result.Error.Code);
        await _postRepository.DidNotReceive().DeletePostAsync(Arg.Any<Post>());
    }

    [Fact]
    public async Task DeletePost_WhenIsAuthor_DeletesAndReturnsSuccess()
    {
        // Arrange
        var post = new Post { Id = Guid.NewGuid(), UserProfileId = Guid.NewGuid() };
        _postRepository.GetPostAsync(post.Id).Returns(post);
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserProfileId.Returns(post.UserProfileId.ToString());

        // Act
        var result = await _sut.DeletePost(post.Id);

        // Assert
        await _postRepository.Received(1).DeletePostAsync(post);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetPostById_WhenDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _postRepository.GetPostAsync(Arg.Any<Guid>()).ReturnsNull();

        // Act
        var result = await _sut.GetPostById(Guid.NewGuid());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PostErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetPostById_WithExistingPost_ReturnsPost()
    {
        // Arrange
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Content = "",
            UserProfile = new UserProfile { Id = new Guid(), Username = "username" },
            Tags = new List<Tag>{ new Tag { TagName = "tag1" }, new Tag { TagName = "tag2" } }
        };
        _postRepository.GetPostAsync(post.Id).Returns(post);

        // Act
        var result = await _sut.GetPostById(post.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(post.Id, result.Value.Id);
        Assert.Equal(post.Title, result.Value.Title);
        Assert.Equal(post.Content, result.Value.Content);
        Assert.Equal(post.UserProfile.Username, result.Value.Author.Username);
        Assert.Equal(post.Tags.Count, result.Value.Tags.Count);
    }

    [Fact]
    public async Task UpdatePost_WithEmptyTitle_ReturnsValidationFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdatePostDto { Title = string.Empty };
        var validation = new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Title", "Title cant be empty") });
        _updatePostDtoValidator.ValidateAsync(dto).Returns(validation);

        // Act
        var result = await _sut.UpdatePost(id, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        await _postRepository.DidNotReceive().GetPostAsync(id);
        await _postRepository.DidNotReceive().UpdatePostAsync(Arg.Any<Post>());
    }

    [Fact]
    public async Task UpdatePost_WhenNotAuthenticated_ReturnsUserNotFound()
    {
        // Arrange
        _updatePostDtoValidator.ValidateAsync(Arg.Any<UpdatePostDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(false);
        _userContext.UserProfileId.Returns((string?)null);

        // Act
        var result = await _sut.UpdatePost(Guid.NewGuid(), new UpdatePostDto());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrors.UserNotFound.Code, result.Error.Code);
        await _postRepository.DidNotReceive().GetPostAsync(Arg.Any<Guid>());
        await _postRepository.DidNotReceive().UpdatePostAsync(Arg.Any<Post>());
    }

    [Fact]
    public async Task UpdatePost_WithNonExistingPost_ReturnsNotFound()
    {
        // Arrange
        _updatePostDtoValidator.ValidateAsync(Arg.Any<UpdatePostDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserProfileId.Returns(Guid.NewGuid().ToString());
        _postRepository.GetPostAsync(Arg.Any<Guid>()).ReturnsNull();

        // Act
        var result = await _sut.UpdatePost(Guid.NewGuid(), new UpdatePostDto());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PostErrors.NotFound.Code, result.Error.Code);
        await _postRepository.DidNotReceive().UpdatePostAsync(Arg.Any<Post>());
    }

    [Fact]
    public async Task UpdatePost_WhenIsNotAuthor_ReturnsForbidden()
    {
        // Arrange
        var post = new Post { Id = Guid.NewGuid(), UserProfileId = Guid.NewGuid() };
        _postRepository.GetPostAsync(post.Id).Returns(post);
        _updatePostDtoValidator.ValidateAsync(Arg.Any<UpdatePostDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserProfileId.Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _sut.UpdatePost(post.Id, new UpdatePostDto());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PostErrors.Forbidden.Code, result.Error.Code);
        await _postRepository.DidNotReceive().UpdatePostAsync(Arg.Any<Post>());
    }

    [Fact]
    public async Task UpdatePost_WhenIsAuthorAndValidDto_UpdatesAndReturnsSuccess()
    {
        // Arrange
        
        var post = new Post
        {
            Id = Guid.NewGuid(),
            UserProfileId = Guid.NewGuid(),
            Title = "Old title",
            UserProfile = new UserProfile { Id = Guid.NewGuid(), Username = "username" },
            Tags = new List<Tag> { new Tag { TagName = "tag1" }, new Tag { TagName = "tag2" } }
        };
        _postRepository.GetPostAsync(post.Id).Returns(post);
        _updatePostDtoValidator.ValidateAsync(Arg.Any<UpdatePostDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserProfileId.Returns(post.UserProfileId.ToString());
        
        var dtoTagList = new List<string> { "tag3", "tag4" };
        var updatePostDto = new UpdatePostDto { Title = "New title", Tags = dtoTagList };
        var tagList = dtoTagList.Select(t => new Tag { TagName = t }).ToList();
        _tagService.ResolveTagsAsync(updatePostDto.Tags).Returns(tagList);

        // Act
        var result = await _sut.UpdatePost(post.Id, updatePostDto);

        // Assert
        await _postRepository.Received(1).UpdatePostAsync(post);
        Assert.True(result.IsSuccess);
        Assert.Equal(updatePostDto.Title, result.Value.Title);
        Assert.Equal(dtoTagList.Order(), result.Value.Tags.Order());
        Assert.Equal(post.Content, result.Value.Content);
    }

    [Fact]
    public async Task GetAllPosts_WithValidQuery_ReturnsPagedList()
    {
        // Arrange
        var queryParamsDto = new PostQueryParametersDto { Page = 1, PageSize = 10 };
        _postQueryParameterDtoValidator.ValidateAsync(queryParamsDto).Returns(new ValidationResult());

        var posts = new List<Post>
    {
        new Post
        {
            Id = Guid.NewGuid(),
            Title = "Post title",
            Slug = "post-title",
            Excerpt = "Excerpt",
            Content = "Content",
            CreatedAt = DateTime.UtcNow,
            UserProfile = new UserProfile { Id = Guid.NewGuid(), Username = "username" },
            Tags = new List<Tag> { new Tag { TagName = "tag" } }
        }
    };

        _postRepository.GetPostQuery().Returns(posts.BuildMock());

        // Act
        var result = await _sut.GetAllPosts(queryParamsDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(posts[0].Title, result.Value.Items[0].Title);
    }
}
