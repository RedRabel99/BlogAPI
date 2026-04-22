using BlogAPI.Application.DTOs.Auth;
using BlogAPI.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace BlogAPI.IntegrationTests.Auth;

public class AuthIntegrationTests : BaseIntegrationTest
{
    public AuthIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        //Arrange
        var registerDto = new RegisterDto
        {
            Email = "newuser@mail.com",
            Username = "newuser",
            DisplayName = "newuser",
            Password = DataSeeder.DefaultPassword
        };
        var existingUsersBeforeRegistration = AppDbContext.Users.Count();

        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/register", registerDto);

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(existingUsersBeforeRegistration + 1, AppDbContext.Users.Count());
        Assert.True(AppDbContext.Users.Any(u => u.Email == registerDto.Email && u.UserName == registerDto.Username));
    }

    [Fact]
    public async Task Register_WithInvalidData_ReturnsBadRequest()
    {
        //Arrange
        var registerDto = new RegisterDto
        {
            Email = "invalid-email",
            Username = "newuser",
            DisplayName = "newuser",
            Password = "invalid-password"
        };
        var existingUsersBeforeRegistration = AppDbContext.Users.Count();
        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/register", registerDto);

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(existingUsersBeforeRegistration, AppDbContext.Users.Count());
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        //Arrange
        var existingUser = DataSeeder.GetApplicationUser();

        var registerDto = new RegisterDto
        {
            Email = existingUser.Email,
            Username = "username",
            DisplayName = "username",
            Password = DataSeeder.DefaultPassword,
        };
        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/register", registerDto);
        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsConflict()
    {
        //Arrange
        var existingUser = DataSeeder.GetApplicationUser();

        var registerDto = new RegisterDto
        {
            Email = "newuser@mail.com",
            Username = existingUser.UserName,
            DisplayName = "username",
            Password = DataSeeder.DefaultPassword,
        };
        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/register", registerDto);
        var body = await response.Content.ReadAsStringAsync();
        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = DataSeeder.DefaultPassword
        };
        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/login", dto);
        var token = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_NonExistingEmail_ReturnsUnauthorized()
    {
        //Arrange
        var registerDto = new LoginDto
        {
            Email = $"newuser@mail.com",
            Password = DataSeeder.DefaultPassword
        };

        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/login", registerDto);

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        //Arrange
        var existingUser = DataSeeder.GetApplicationUser();
        var registerDto = new LoginDto
        {
            Email = existingUser.Email,
            Password = "WrongPassword"
        };

        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/login", registerDto);

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangeUsername_Authenticated_ReturnsOk()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        var changeDto = new ChangeUsernameDto
        {
            Username = "newchangeusername"
        };
        //Act
        var response = await HttpClient.PatchAsJsonAsync("auth/username", changeDto);

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedUserEntity = AppDbContext.Users.AsNoTracking().Include(u => u.UserProfile).First(u => u.Id == user.Id);
        Assert.Equal(updatedUserEntity.UserName, changeDto.Username);
        Assert.Equal(updatedUserEntity.UserProfile.Username, changeDto.Username);
    }

    [Fact]
    public async Task ChangeUsername_WithDuplicateUsername_ReturnsConflict()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        var anotherUser = DataSeeder.GetApplicationUser(1);
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        var changeDto = new ChangeUsernameDto
        {
            Username = anotherUser.UserName
        };
        //Act
        var response = await HttpClient.PatchAsJsonAsync("auth/username", changeDto);
        var body = await response.Content.ReadAsStringAsync();
        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var updatedUserEntity = AppDbContext.Users.AsNoTracking().Include(u => u.UserProfile).First(u => u.Id == user.Id);
        Assert.NotEqual(updatedUserEntity.UserName, changeDto.Username);
        Assert.NotEqual(updatedUserEntity.UserProfile.Username, changeDto.Username);
    }

    [Fact]
    public async Task ChangeUsername_InvalidData_ReturnsBadRequest()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        //Act
        var dto = new ChangeUsernameDto { Username = "" };
        var response = await HttpClient.PatchAsJsonAsync("auth/username", dto);

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_Authenticated_ReturnsOk()
    {
        //Arrange
        //Creating new uset to not change default password in other tests
        var newUserDto = new RegisterDto
        {
            Email = "changepassword@mail.com",
            Username = "changepassworduser",
            DisplayName = "changepassworduser",
            Password = DataSeeder.DefaultPassword
        };
        await UserManager.CreateUserAsync(newUserDto);
        await AuthenticateAsync(newUserDto.Email, DataSeeder.DefaultPassword);

        var changeDto = new ChangePasswordDto
        {
            OldPassword = DataSeeder.DefaultPassword,
            NewPassword = "NewPassword123!"
        };

        //Act
        var response = await HttpClient.PatchAsJsonAsync("auth/password", changeDto);

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //Check wether updated password works
        HttpClient.DefaultRequestHeaders.Authorization = null;
        response = await HttpClient.PostAsJsonAsync<LoginDto>("auth/login", new() { Email = newUserDto.Email, Password = changeDto.NewPassword });
        var token = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(token);
        Assert.False(string.IsNullOrEmpty(token));
    }

    [Fact]
    public async Task ChangePassword_WithInvalidNewPassword_ReturnsBadRequest()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);
        var changeDto = new ChangePasswordDto
        {
            OldPassword = DataSeeder.DefaultPassword,
            NewPassword = "short"
        };
        //Act
        var response = await HttpClient.PatchAsJsonAsync("auth/password", changeDto);

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithWrongOldPassword_ReturnsBadRequest()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);
        var changeDto = new ChangePasswordDto
        {
            OldPassword = "WrongOldPassword",
            NewPassword = "NewPassword123!"
        };
        //Act
        var response = await HttpClient.PatchAsJsonAsync("auth/password", changeDto);

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateChangeEmailToken_Authenticated_ReturnsToken()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);
        var generateTokenDto = new GenerateChangeEmailTokenDto
        {
            Email = "generatenew@mail.com"
        };

        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/email/change-token", generateTokenDto);
        var body = await response.Content.ReadFromJsonAsync<ChangeEmailTokenResponseDto>();
        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrEmpty(body.Token));
    }

    [Fact]
    public async Task GenerateChangeEmailToken_WithInvalidEmail_ReturnsBadRequest()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);
        var generateTokenDto = new GenerateChangeEmailTokenDto
        {
            Email = "invalid-email"
        };
        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/email/change-token", generateTokenDto);

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangeEmail_WithValidToken_ReturnsOk()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        var newEmail = $"newchange@mail.com";
        var token = UserManager.GenerateChangeEmailTokenAsync(user.Id, newEmail).Result.Value;

        var dto = new ChangeEmailDto
        {
            Email = newEmail,
            Token = token
        };
        //Act
        var response = await HttpClient.PatchAsJsonAsync("auth/email", dto);

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedUserEntity = AppDbContext.Users.AsNoTracking().Include(u => u.UserProfile).First(u => u.Id == user.Id);
        Assert.Equal(updatedUserEntity.Email, newEmail);
    }

    [Fact]
    public async Task ChangeEmail_WithInvalidToken_ReturnsBadRequest()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        var newEmail = $"newchange@mail.com";
        await UserManager.GenerateChangeEmailTokenAsync(user.Id, newEmail);

        var dto = new ChangeEmailDto
        {
            Email = newEmail,
            Token = "invalid-token"
        };

        //Act
        var response = await HttpClient.PatchAsJsonAsync("auth/email", dto);

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}