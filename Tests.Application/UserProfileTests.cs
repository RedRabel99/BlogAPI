using System.Reflection;
using BlogAPI.Application.DTOs.UserProfile;
using BlogAPI.Application.DTOs.UserProfiles;
using BlogAPI.Application.Services;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.UserProfiles;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Tests.Application;

public class UserProfileServiceTests
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserContext _userContext;
    private readonly IValidator<UpdateUserProfileDto> _updateValidator;
    private readonly IValidator<UserProfileQueryParametersDto> _queryParametersValidator;
    private readonly IPagedListFactory _pagedListFactory;
    private readonly UserProfileService _sut;

    public UserProfileServiceTests()
    {
        _userProfileRepository = Substitute.For<IUserProfileRepository>();
        _userContext = Substitute.For<IUserContext>();
        _updateValidator = Substitute.For<IValidator<UpdateUserProfileDto>>();
        _queryParametersValidator = Substitute.For<IValidator<UserProfileQueryParametersDto>>();
        _pagedListFactory = Substitute.For<IPagedListFactory>();
        _sut = new UserProfileService(_userProfileRepository, _userContext, _updateValidator, _queryParametersValidator, _pagedListFactory);
    }

    [Fact]
    public async Task GetUserProfileById_WithExistingId_ReturnsUserProfile()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userProfile = new UserProfile { Id = id, Username = "User1", DisplayName = "User1", ApplicationUserId = "identity-id" };
        _userProfileRepository.GetByIdAsync(id).Returns(userProfile);

        // Act
        var result = await _sut.GetUserProfileById(id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Value);
        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(userProfile.ApplicationUserId, result.Value.ApplicationUserId);
        Assert.Equal(userProfile.Username, result.Value.UserName);
        Assert.Equal(userProfile.DisplayName, result.Value.DisplayName);
    }

    [Fact]
    public async Task GetUserProfileById_WithNonExistingId_ReturnsFailure()
    {
        // Arrange
        _userProfileRepository.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

        // Act
        var result = await _sut.GetUserProfileById(Guid.NewGuid());

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsError);
        Assert.NotNull(result.Error);
        Assert.Null(result.Value);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(UserProfileErrors.NotFound.Code, result.Error.Code);
        Assert.Equal(UserProfileErrors.NotFound.Description, result.Error.Description);
    }

    [Fact]
    public async Task DeleteUserProfileById_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        _userProfileRepository.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

        // Act
        var result = await _sut.DeleteUserProfileById(Guid.NewGuid());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task DeleteUserProfileById_WithDifferentUser_ReturnsForbidden()
    {
        // Arrange
        var id = Guid.NewGuid();
        var profile = new UserProfile { Id = id, ApplicationUserId = "owner-id" };
        _userProfileRepository.GetByIdAsync(id).Returns(profile);
        _userContext.UserId.Returns("different-id");

        // Act
        var result = await _sut.DeleteUserProfileById(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }

    [Fact]
    public async Task DeleteUserProfileById_WithOwner_DeletesAndReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var profile = new UserProfile { Id = id, ApplicationUserId = "owner-id" };
        _userProfileRepository.GetByIdAsync(id).Returns(profile);
        _userContext.UserId.Returns("owner-id");

        // Act
        var result = await _sut.DeleteUserProfileById(id);

        // Assert
        await _userProfileRepository.Received(1).DeleteAsync(id);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_WithNoUserContext_ReturnsNotFound()
    {
        // Arrange
        _userContext.UserId.Returns((string?)null);

        // Act
        var result = await _sut.GetCurrentUserProfileAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_WithExisting_ReturnsProfile()
    {
        // Arrange
        var profile = new UserProfile { Id = Guid.NewGuid(), ApplicationUserId = "app-id", Username = "u", DisplayName = "d" };
        _userContext.UserId.Returns("app-id");
        _userProfileRepository.GetByApplicationUserIdAsync("app-id").Returns(profile);

        // Act
        var result = await _sut.GetCurrentUserProfileAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(profile.Id, result.Value.Id);
    }

    [Fact]
    public async Task GetUserProfileByUsername_WithExisting_ReturnsSuccess()
    {
        // Arrange
        var profile = new UserProfile { Id = Guid.NewGuid(), Username = "name", ApplicationUserId = "a" };
        _userProfileRepository.GetByUsername("name").Returns(profile);

        // Act
        var result = await _sut.GetUserProfileByUsername("name");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(profile.Id, result.Value.Id);
    }

    [Fact]
    public async Task GetUserProfileByUsername_WithNotFound_ReturnsFailure()
    {
        // Arrange
        _userProfileRepository.GetByUsername(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _sut.GetUserProfileByUsername("x");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task GetUserProfiles_WithInvalidQuery_ReturnsValidationFailure()
    {
        // Arrange
        var qp = new UserProfileQueryParametersDto();
        var failures = new List<ValidationFailure> { new ValidationFailure("UserName", "err") };
        var validation = new ValidationResult(failures);
        _queryParametersValidator.Validate(qp).Returns(validation);

        // Act
        var result = await _sut.GetUserProfiles(qp);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.NotEmpty(result.SubErrors);
    }

    [Fact]
    public async Task GetUserProfiles_WithValidQuery_ReturnsPagedList()
    {
        // Arrange
        var qp = new UserProfileQueryParametersDto { Page = 1, PageSize = 10 };
        _queryParametersValidator.Validate(qp).Returns(new ValidationResult());

        var users = new List<UserProfile> { new UserProfile { Id = Guid.NewGuid(), Username = "u", DisplayName = "d", ApplicationUserId = "a" } };
        _userProfileRepository.GetAll().Returns(users.AsQueryable());

        var items = new List<UserProfileDto> { new UserProfileDto { Id = users[0].Id, ApplicationUserId = "a", DisplayName = "d", UserName = "u" } };
        var pagedList = (PagedList<UserProfileDto>)Activator.CreateInstance(typeof(PagedList<UserProfileDto>), BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { items, 1, 10, items.Count }, null)!;
        _pagedListFactory.CreateAsync(Arg.Any<IQueryable<UserProfileDto>>(), qp.Page, qp.PageSize).Returns(Task.FromResult(pagedList));

        // Act
        var result = await _sut.GetUserProfiles(qp);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(items.Count, result.Value.Items.Count);
    }

    [Fact]
    public async Task UpdateUserProfile_WhenValidationFails_ReturnsValidationFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateUserProfileDto { DisplayName = "d" };
        var validation = new ValidationResult(new List<ValidationFailure> { new ValidationFailure("DisplayName", "err") });
        _updateValidator.Validate(dto).Returns(validation);

        // Act
        var result = await _sut.UpdateUserProfile(id, dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task UpdateUserProfile_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        _updateValidator.Validate(Arg.Any<UpdateUserProfileDto>()).Returns(new ValidationResult());
        _userProfileRepository.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

        // Act
        var result = await _sut.UpdateUserProfile(Guid.NewGuid(), new UpdateUserProfileDto());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task UpdateUserProfile_WhenForbidden_ReturnsForbidden()
    {
        // Arrange
        var id = Guid.NewGuid();
        _updateValidator.Validate(Arg.Any<UpdateUserProfileDto>()).Returns(new ValidationResult());
        var entity = new UserProfile { Id = id, ApplicationUserId = "owner" };
        _userProfileRepository.GetByIdAsync(id).Returns(entity);
        _userContext.UserId.Returns("other");

        // Act
        var result = await _sut.UpdateUserProfile(id, new UpdateUserProfileDto { DisplayName = "x" });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }

    [Fact]
    public async Task UpdateUserProfile_WhenUpdateReturnsNull_ReturnsInternal()
    {
        // Arrange
        var id = Guid.NewGuid();
        _updateValidator.Validate(Arg.Any<UpdateUserProfileDto>()).Returns(new ValidationResult());
        var entity = new UserProfile { Id = id, ApplicationUserId = "owner", DisplayName = "old" };
        _userProfileRepository.GetByIdAsync(id).Returns(entity);
        _userContext.UserId.Returns("owner");
        _userProfileRepository.UpdateAsync(Arg.Any<UserProfile>()).ReturnsNull();

        // Act
        var result = await _sut.UpdateUserProfile(id, new UpdateUserProfileDto { DisplayName = "new" });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Internal, result.Error.Type);
    }

    [Fact]
    public async Task UpdateUserProfile_WhenSuccessful_ReturnsUpdatedDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        _updateValidator.Validate(Arg.Any<UpdateUserProfileDto>()).Returns(new ValidationResult());
        var entity = new UserProfile { Id = id, ApplicationUserId = "owner", DisplayName = "old", Username = "u" };
        _userProfileRepository.GetByIdAsync(id).Returns(entity);
        _userContext.UserId.Returns("owner");
        var updated = new UserProfile { Id = id, ApplicationUserId = "owner", DisplayName = "new", Username = "u" };
        _userProfileRepository.UpdateAsync(Arg.Any<UserProfile>()).Returns(updated);

        // Act
        var result = await _sut.UpdateUserProfile(id, new UpdateUserProfileDto { DisplayName = "new" });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("new", result.Value.DisplayName);
    }
}
