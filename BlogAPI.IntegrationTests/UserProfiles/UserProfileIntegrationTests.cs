using BlogAPI.Application.DTOs.UserProfile;
using BlogAPI.Application.DTOs.UserProfiles;
using BlogAPI.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Json;

namespace BlogAPI.IntegrationTests.UserProfiles;

public class UserProfileIntegrationTests : BaseIntegrationTest
{
    public UserProfileIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task GetUserProfile_WithExistingUsername_ReturnsUserProfile()
    {
        //Arrange
        var userProfile = DataSeeder.GetUserProfile();

        //Act
        var response = await HttpClient.GetAsync($"userprofile/{userProfile.Username}");

        //Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseUserProfile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(responseUserProfile);
        Assert.Equal(userProfile.Username, responseUserProfile.UserName);
        Assert.Equal(userProfile.Id, responseUserProfile.Id);
        Assert.Equal(userProfile.ApplicationUserId, responseUserProfile.ApplicationUserId);
        Assert.Equal(userProfile.DisplayName, responseUserProfile.DisplayName);
    }

    [Fact]
    public async Task GetUserProfile_WithNonExistingUsername_ReturnsNotFound()
    {
        //Arrange
        //Using new guid to guarantee that the username does not exist in the seeded data
        var nonExistingUsername = Guid.NewGuid().ToString();

        //Act
        var response = await HttpClient.GetAsync($"userprofile/{nonExistingUsername}");

        //Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserProfile_WithQueryParams_RetunsUserProfileList()
    {
        //Arrange
        var queryParams = new UserProfileQueryParametersDto()
        {
            PageSize = DataSeeder.GetUserProfilesLength()
        };

        var query = QueryString.Create("pageSize", queryParams.PageSize.ToString());
        //Act

        var response = await HttpClient.GetAsync("userprofile" + query);
        var body = await response.Content.ReadFromJsonAsync<PagedListResponse<UserProfileDto>>();

        //Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(DataSeeder.GetUserProfilesLength(), body.Items.Count);
    }

    [Fact]
    public async Task GetUserProfiles_WithSecondPageQueryParams_ReturnsCorrectPagedList()
    {
        //Arrange
        var queryParams = new UserProfileQueryParametersDto()
        {
            PageSize = 1,
            Page = 2
        };

        var query = QueryString.Create(new Dictionary<string, string?>
        {
            ["PageSize"] = queryParams.PageSize.ToString(),
            ["Page"] = queryParams.Page.ToString()
        });

        //Act
        var response = await HttpClient.GetAsync("userprofile" + query);
        var body = await response.Content.ReadFromJsonAsync<PagedListResponse<UserProfileDto>>();

        //Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(queryParams.PageSize, body.Items.Count);
        Assert.Equal(queryParams.Page, body.Page);
        var shouldHaveNextPage = DataSeeder.GetUserProfilesLength() / queryParams.PageSize > queryParams.Page;
        Assert.Equal(shouldHaveNextPage, body.HasNextPage);
        Assert.Equal(queryParams.Page > 1, body.HasPreviousPage);
    }

    [Fact]
    public async Task GetCurrentUserProfile_WhenAuthenticated_ReturnsOwnProfile()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        //Act
        var response = await HttpClient.GetAsync("userprofile/me");
        var body = await response.Content.ReadFromJsonAsync<UserProfileDto>();

        //Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(user.Id, body.ApplicationUserId);
        Assert.Equal(user.UserName, body.UserName);
        Assert.Equal(user.UserProfile.DisplayName, body.DisplayName);
        Assert.Equal(user.UserProfile.Id, body.Id);
    }

    [Fact]
    public async Task GetCurrentProfile_Unauthenticated_ReturnsUnauthorized()
    {
        //Arrange
        HttpClient.DefaultRequestHeaders.Authorization = null;

        //Act
        var response = await HttpClient.GetAsync("userprofile/me");

        //Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserProfile_AuthenticatedAsOwner_ReturnsUpdatedProfile()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);
        var updateDto = new UpdateUserProfileDto
        {
            DisplayName = "New Username"
        };

        //Act
        var response = await HttpClient.PatchAsJsonAsync($"userprofile/{user.UserProfile.Id}", updateDto);
        var body = await response.Content.ReadFromJsonAsync<UserProfileDto>();

        //Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(updateDto.DisplayName, body.DisplayName);
    }

    [Fact]
    public async Task UpdateUserProfile_AuthenticatedAsNonOwner_ReturnsForbidden()
    {
        //Arrange
        var user1 = DataSeeder.GetApplicationUser(0);
        var user2 = DataSeeder.GetApplicationUser(1);
        await AuthenticateAsync(user1.Email, DataSeeder.DefaultPassword);

        var updateDto = new UpdateUserProfileDto
        {
            DisplayName = "New Username"
        };

        //Act
        var response = await HttpClient.PatchAsJsonAsync($"userprofile/{user2.UserProfile.Id}", updateDto);

        //Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}