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
        //Arrange
        var id = Guid.NewGuid();
        var userProfile = new UserProfile
        {
            Id = id,
            Username = "User1",
            DisplayName = "User1",
            ApplicationUserId = "identity-id"
        };
        _userProfileRepository.GetByIdAsync(id).Returns(userProfile);
        
        //Act
        var result = await _sut.GetUserProfileById(id);

        //Assert
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
        //Arrange
        _userProfileRepository.GetByIdAsync(Arg.Any<Guid>()).ReturnsNull();

        //Act
        var result = await _sut.GetUserProfileById(Guid.NewGuid());

        //Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsError);
        Assert.NotNull(result.Error);
        Assert.Null(result.Value);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(UserProfileErrors.NotFound.Code, result.Error.Code);
        Assert.Equal(UserProfileErrors.NotFound.Description, result.Error.Description);
    }
}
