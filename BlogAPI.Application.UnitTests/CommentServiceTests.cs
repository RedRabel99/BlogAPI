using BlogAPI.Application.DTOs.Comments;
using BlogAPI.Application.Services;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.Comments;
using BlogAPI.Domain.Interfaces.Posts;
using BlogAPI.Domain.Interfaces.UserProfiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using MockQueryable;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Tests.Application;

public class CommentServiceTests
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IPagedListFactory _pagedListFactory;
    private readonly IValidator<CreateCommentDto> _createCommentValidator;
    private readonly IValidator<UpdateCommentDto> _updateCommentValidator;
    private readonly IValidator<CommentQueryParametersDto> _commentQueryParametersValidator;
    private readonly IUserContext _userContext;
    private readonly CommentService _sut;

    public CommentServiceTests()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _postRepository = Substitute.For<IPostRepository>();
        _userProfileRepository = Substitute.For<IUserProfileRepository>();
        _pagedListFactory = new PagedListFactory(Options.Create(new PaginationOptions { DefaultPage = 1, DefaultPageSize = 10 }));
        _createCommentValidator = Substitute.For<IValidator<CreateCommentDto>>();
        _updateCommentValidator = Substitute.For<IValidator<UpdateCommentDto>>();
        _commentQueryParametersValidator = Substitute.For<IValidator<CommentQueryParametersDto>>();
        _userContext = Substitute.For<IUserContext>();

        _sut = new CommentService(
            _commentRepository,
            _postRepository,
            _userProfileRepository,
            _pagedListFactory,
            _createCommentValidator,
            _updateCommentValidator,
            _commentQueryParametersValidator,
            _userContext);
    }

    [Fact]
    public async Task CreateComment_WithEmptyContent_ReturnsValidationFailure()
    {
        //Arrange
        var dto = new CreateCommentDto { Content = string.Empty };
        var failures = new[] { new ValidationFailure("Content", "Content is required.") };
        _createCommentValidator.ValidateAsync(Arg.Any<CreateCommentDto>()).Returns(new ValidationResult(failures));

        //Act
        var result = await _sut.CreateCommentAsync(Guid.NewGuid(), dto);


        //Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        await _commentRepository.DidNotReceive().CreateAsync(Arg.Any<Comment>());
    }

    [Fact]
    public async Task CreateComment_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        //Arrange
        _createCommentValidator.ValidateAsync(Arg.Any<CreateCommentDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(false);
        _userContext.UserId.Returns((string?)null);

        //Act
        var result = await _sut.CreateCommentAsync(Guid.NewGuid(), new CreateCommentDto { Content = "x" });

        //Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(CommentErrors.Unauthorized.Code, result.Error.Code);
    }

    [Fact]
    public async Task CreateComment_WhenPostDoesNotExist_ReturnsPostNotFound()
    {
        //Arrange
        _createCommentValidator.ValidateAsync(Arg.Any<CreateCommentDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns("user-id");
        _postRepository.GetPostAsync(Arg.Any<Guid>()).ReturnsNull();

        //Act
        var result = await _sut.CreateCommentAsync(Guid.NewGuid(), new CreateCommentDto { Content = "x" });

        //Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PostErrors.NotFound.Code, result.Error.Code);
        await _commentRepository.DidNotReceive().CreateAsync(Arg.Any<Comment>());
    }

    [Fact]
    public async Task CreateComment_WhenUserProfileNotExists_ReturnsUserProfileNotFound()
    {
        //Arrange
        _createCommentValidator.ValidateAsync(Arg.Any<CreateCommentDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns("user-id");
        _postRepository.GetPostAsync(Arg.Any<Guid>()).Returns(new Post { Id = Guid.NewGuid() });
        _userProfileRepository.GetByApplicationUserIdAsync("user-id").ReturnsNull();

        //Act
        var result = await _sut.CreateCommentAsync(Guid.NewGuid(), new CreateCommentDto { Content = "x" });

        //Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(UserProfileErrors.NotFound.Code, result.Error.Code);
        await _commentRepository.DidNotReceive().CreateAsync(Arg.Any<Comment>());
    }

    [Fact]
    public async Task CreateComment_WithValidData_ReturnsCommentDto()
    {
        //Arrange
        var postId = Guid.NewGuid();
        var userProfile = new UserProfile { Id = Guid.NewGuid(), Username = "alice", ApplicationUserId = "user-id" };
        var dto = new CreateCommentDto { Content = "Hello world" };

        _createCommentValidator.ValidateAsync(Arg.Any<CreateCommentDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserId.Returns(userProfile.ApplicationUserId);
        _postRepository.GetPostAsync(postId).Returns(new Post { Id = postId });
        _userProfileRepository.GetByApplicationUserIdAsync(userProfile.ApplicationUserId).Returns(userProfile);
        _commentRepository.CreateAsync(Arg.Any<Comment>()).Returns(call => call.Arg<Comment>());

        //Act
        var result = await _sut.CreateCommentAsync(postId, dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(dto.Content, result.Value!.Content);
        Assert.Equal(postId, result.Value.PostId);
        Assert.Equal(userProfile.Username, result.Value.Author.Username);
    }

    [Fact]
    public async Task GetCommentById_WhenCommentNotExists_ReturnsNotFound()
    {
        //Arrange
        _commentRepository.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

        //Act
        var result = await _sut.GetCommentByIdAsync(Guid.NewGuid());

        //Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(CommentErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetCommentById_WhenCommentExists_ReturnsCommentDto()
    {
        //Arrange
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = "Hi",
            PostId = Guid.NewGuid(),
            UserProfile = new UserProfile { Id = Guid.NewGuid(), Username = "alice" }
        };
        _commentRepository.GetByIdAsync(comment.Id).Returns(comment);

        //Act
        var result = await _sut.GetCommentByIdAsync(comment.Id);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(comment.Id, result.Value!.Id);
        Assert.Equal(comment.Content, result.Value.Content);
        Assert.Equal(comment.UserProfile.Username, result.Value.Author.Username);
    }

    [Fact]
    public async Task GetCommentsByPostId_WithInvalidQuery_ReturnsValidationFailure()
    {
        //Arrange
        var queryParams = new CommentQueryParametersDto();
        var failures = new[] { new ValidationFailure("PageSize", "Error") };
        _commentQueryParametersValidator.ValidateAsync(queryParams).Returns(new ValidationResult(failures));

        //Act
        var result = await _sut.GetCommentsByPostIdAsync(Guid.NewGuid(), queryParams);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task GetCommentsByPostId_WhenPostDoesNotExist_ReturnsPostNotFound()
    {
        //Arrange
        _commentQueryParametersValidator.ValidateAsync(Arg.Any<CommentQueryParametersDto>()).Returns(new ValidationResult());
        _postRepository.GetPostAsync(Arg.Any<Guid>()).ReturnsNull();

        //Act
        var result = await _sut.GetCommentsByPostIdAsync(Guid.NewGuid(), new CommentQueryParametersDto());

        //Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PostErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetCommentsByPostId_WithValidQuery_ReturnsPagedList()
    {
        //Arrange
        var postId = Guid.NewGuid();
        var queryParams = new CommentQueryParametersDto { Page = 1, PageSize = 10 };
        _commentQueryParametersValidator.ValidateAsync(queryParams).Returns(new ValidationResult());
        _postRepository.GetPostAsync(postId).Returns(new Post { Id = postId });

        var comments = new List<Comment>
        {
            new Comment
            {
                Id = Guid.NewGuid(),
                Content = "First",
                PostId = postId,
                CreatedAt = DateTime.UtcNow,
                UserProfile = new UserProfile { Id = Guid.NewGuid(), Username = "alice" }
            }
        };
        _commentRepository.GetQuery().Returns(comments.BuildMock());

        //Act
        var result = await _sut.GetCommentsByPostIdAsync(postId, queryParams);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalCount);
        Assert.Equal(comments[0].Content, result.Value.Items[0].Content);
    }

    [Fact]
    public async Task UpdateComment_WithEmptyContent_ReturnsValidationFailure()
    {
        //Arrange
        var dto = new UpdateCommentDto { Content = string.Empty };
        var failures = new[] { new ValidationFailure("Content", "Required") };
        _updateCommentValidator.ValidateAsync(dto).Returns(new ValidationResult(failures));

        //Act
        var result = await _sut.UpdateCommentAsync(Guid.NewGuid(), dto);
        
        //Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>());
    }

    [Fact]
    public async Task UpdateComment_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        //Arrange
        _updateCommentValidator.ValidateAsync(Arg.Any<UpdateCommentDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(false);
        _userContext.UserProfileId.Returns((string?)null);

        //Act
        var result = await _sut.UpdateCommentAsync(Guid.NewGuid(), new UpdateCommentDto { Content = "x" });

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CommentErrors.Unauthorized.Code, result.Error.Code);
    }

    [Fact]
    public async Task UpdateComment_WhenNotFound_ReturnsNotFound()
    {
        //Arrange
        _updateCommentValidator.ValidateAsync(Arg.Any<UpdateCommentDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserProfileId.Returns(Guid.NewGuid().ToString());
        _commentRepository.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

        //Act
        var result = await _sut.UpdateCommentAsync(Guid.NewGuid(), new UpdateCommentDto { Content = "x" });

        //Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(CommentErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task UpdateComment_WhenNotAuthor_ReturnsForbidden()
    {
        //Arrange
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            UserProfileId = Guid.NewGuid(),
            Content = "Old",
            UserProfile = new UserProfile { Id = Guid.NewGuid(), Username = "alice" }
        };
        _updateCommentValidator.ValidateAsync(Arg.Any<UpdateCommentDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserProfileId.Returns(Guid.NewGuid().ToString());
        _commentRepository.GetByIdAsync(comment.Id).Returns(comment);

        //Act
        var result = await _sut.UpdateCommentAsync(comment.Id, new UpdateCommentDto { Content = "New" });

        //Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(CommentErrors.Forbidden.Code, result.Error.Code);
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>());
    }

    [Fact]
    public async Task UpdateComment_WhenAuthor_UpdatesAndReturnsSuccess()
    {
        //Arrange
        var userProfileId = Guid.NewGuid();
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfileId,
            Content = "Old",
            UserProfile = new UserProfile { Id = userProfileId, Username = "alice" }
        };
        _updateCommentValidator.ValidateAsync(Arg.Any<UpdateCommentDto>()).Returns(new ValidationResult());
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserProfileId.Returns(userProfileId.ToString());
        _commentRepository.GetByIdAsync(comment.Id).Returns(comment);

        //Act
        var dto = new UpdateCommentDto { Content = "New content" };
        var result = await _sut.UpdateCommentAsync(comment.Id, dto);

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(dto.Content, result.Value!.Content);
        await _commentRepository.Received(1).UpdateAsync(comment);
    }

    [Fact]
    public async Task DeleteComment_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        //Arrange
        _userContext.IsAuthenticated.Returns(false);
        _userContext.UserProfileId.Returns((string?)null);

        //Act
        var result = await _sut.DeleteCommentAsync(Guid.NewGuid());

        //Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(CommentErrors.Unauthorized.Code, result.Error.Code);
    }

    [Fact]
    public async Task DeleteComment_WhenNotFound_ReturnsNotFound()
    {
        //Arrange
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserProfileId.Returns(Guid.NewGuid().ToString());
        _commentRepository.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

        //Act
        var result = await _sut.DeleteCommentAsync(Guid.NewGuid());

        //Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(CommentErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task DeleteComment_WhenNotAuthor_ReturnsForbidden()
    {
        //Arrange
        var comment = new Comment { Id = Guid.NewGuid(), UserProfileId = Guid.NewGuid() };
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserProfileId.Returns(Guid.NewGuid().ToString());
        _commentRepository.GetByIdAsync(comment.Id).Returns(comment);

        //Act
        var result = await _sut.DeleteCommentAsync(comment.Id);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(CommentErrors.Forbidden.Code, result.Error.Code);
        await _commentRepository.DidNotReceive().DeleteAsync(Arg.Any<Comment>());
    }

    [Fact]
    public async Task DeleteComment_WhenAuthor_DeletesAndReturnsSuccess()
    {
        //Arrange
        var userProfileId = Guid.NewGuid();
        var comment = new Comment { Id = Guid.NewGuid(), UserProfileId = userProfileId };
        _userContext.IsAuthenticated.Returns(true);
        _userContext.UserProfileId.Returns(userProfileId.ToString());
        _commentRepository.GetByIdAsync(comment.Id).Returns(comment);

        //Act
        var result = await _sut.DeleteCommentAsync(comment.Id);

        //Assert
        Assert.True(result.IsSuccess);
        await _commentRepository.Received(1).DeleteAsync(comment);
    }
}
