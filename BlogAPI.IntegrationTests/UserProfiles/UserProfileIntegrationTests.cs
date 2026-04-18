using BlogAPI.Application.DTOs.UserProfile;
using BlogAPI.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace BlogAPI.IntegrationTests.UserProfiles;

public class UserProfileIntegrationTests : BaseIntegrationTest
{
    public UserProfileIntegrationTests(IntegrationTestFactory factory) : base(factory){}

    [Fact]
    public async Task GetUserProfile_WithExistingUsername_ReturnsUserProfile()
    {
        //Arrange
        var userProfile = DataSeeder.GetUserProfile();

        //Act
        var response = await HttpClient.GetAsync($"userprofile/{userProfile.Username}");


        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseUserProfile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        Assert.NotNull(responseUserProfile);
        Assert.Equal(userProfile.Username, responseUserProfile.UserName);
        Assert.Equal(userProfile.Id, responseUserProfile.Id);
        Assert.Equal(userProfile.ApplicationUserId, responseUserProfile.ApplicationUserId);
        Assert.Equal(userProfile.DisplayName, responseUserProfile.DisplayName);
    }
}
