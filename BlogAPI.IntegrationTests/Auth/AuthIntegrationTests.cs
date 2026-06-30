using BlogAPI.Application.Auth.Dtos;
using BlogAPI.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace BlogAPI.IntegrationTests.Auth;

public class AuthIntegrationTests : BaseIntegrationTest
{
    public AuthIntegrationTests(IntegrationTestFactory factory) : base(factory) { }

    // ---------------------------------------------------------------------------
    // POST /auth/register
    // ---------------------------------------------------------------------------

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

    // ---------------------------------------------------------------------------
    // POST /auth/login
    // ---------------------------------------------------------------------------

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
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(auth.RefreshToken));
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

    // ---------------------------------------------------------------------------
    // PATCH /auth/username
    // ---------------------------------------------------------------------------

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

    // ---------------------------------------------------------------------------
    // PATCH /auth/password
    // ---------------------------------------------------------------------------

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
        // Register through endpoint for the moment data is not respawned between tests
        await HttpClient.PostAsJsonAsync("auth/register", newUserDto);
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

    // ---------------------------------------------------------------------------
    // POST /auth/email/change-token
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GenerateChangeEmailToken_Authenticated_EnqueuesEmailToOutbox()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);
        var newEmail = "generatenew@mail.com";
        var generateTokenDto = new GenerateChangeEmailTokenDto
        {
            Email = newEmail
        };

        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/email/change-token", generateTokenDto);

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //Materialize in memory first to avoid translation error between string and jsonb
        var enqueued = (await AppDbContext.OutboxMessages.ToListAsync()).Any(m => m.Content.Contains(newEmail));
        Assert.True(enqueued);
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

    // ---------------------------------------------------------------------------
    // PATCH /auth/email
    // ---------------------------------------------------------------------------

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

    // ---------------------------------------------------------------------------
    // POST /auth/forgot-password
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ForgotPassword_WithExistingEmail_EnqueuesEmailToOutbox()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        var dto = new ForgotPasswordDto
        {
            Email = user.Email
        };

        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/forgot-password", dto);

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //Materialize in memory first to avoid translation error between string and jsonb
        var enqueued = (await AppDbContext.OutboxMessages.ToListAsync()).Any(m => m.Content.Contains(user.Email));
        Assert.True(enqueued);
    }

    [Fact]
    public async Task ForgotPassword_WithUnknownEmail_ReturnsOk()
    {
        //Arrange
        var dto = new ForgotPasswordDto
        {
            Email = "doesnotexist@mail.com"
        };

        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/forgot-password", dto);

        //Assert: success even for unknown email, preventing email enumeration
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // POST /auth/reset-password
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ResetPassword_WithValidToken_ChangesPassword()
    {
        //Arrange: dedicated user so resetting password does not affect the shared seeded users
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var username = $"reset_{suffix}";
        var email = $"reset-{suffix}@mail.com";
        var createResult = await UserManager.CreateUserAsync(username, email, DataSeeder.DefaultPassword);
        Assert.True(createResult.IsSuccess);
        AppDbContext.UserProfiles.Add(new BlogAPI.Domain.Entities.UserProfile
        {
            ApplicationUserId = createResult.Value,
            Username = username,
            DisplayName = "Reset User"
        });
        var appUser = await AppDbContext.Users.FirstAsync(u => u.Id == createResult.Value);
        appUser.EmailConfirmed = true;
        await AppDbContext.SaveChangesAsync();

        var token = (await UserManager.GeneratePasswordResetTokenAsync(email)).Value;
        var newPassword = "NewPassword123!";
        var dto = new ResetPasswordDto
        {
            Email = email,
            Token = token,
            NewPassword = newPassword
        };

        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/reset-password", dto);

        //Assert: reset succeeded and login works with OK status code
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loginResponse = await HttpClient.PostAsJsonAsync("auth/login", new LoginDto
        {
            Email = email,
            Password = newPassword
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        var dto = new ResetPasswordDto
        {
            Email = user.Email,
            Token = "invalid-token",
            NewPassword = "NewPassword123!"
        };

        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/reset-password", dto);

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithUnknownEmail_ReturnsBadRequest()
    {
        //Arrange: unknown is bad request to prevent email enumeration
        var dto = new ResetPasswordDto
        {
            Email = "doesnotexist@mail.com",
            Token = "some-token",
            NewPassword = "NewPassword123!"
        };

        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/reset-password", dto);

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // POST /auth/refresh
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewRotatedTokens()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        var auth = await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/refresh",
            new RefreshRequestDto { RefreshToken = auth.RefreshToken });
        var refreshed = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(refreshed);
        Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));
        Assert.NotEqual(auth.RefreshToken, refreshed.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithRotatedToken_ReturnsUnauthorized()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        var auth = await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);
        // first refresh rotates -> revokes the original
        await HttpClient.PostAsJsonAsync("auth/refresh",
            new RefreshRequestDto { RefreshToken = auth.RefreshToken });

        //Act - replay the now-rotated token
        var response = await HttpClient.PostAsJsonAsync("auth/refresh",
            new RefreshRequestDto { RefreshToken = auth.RefreshToken });

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        //Act
        var response = await HttpClient.PostAsJsonAsync("auth/refresh",
            new RefreshRequestDto { RefreshToken = "not-a-real-token" });

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // POST /auth/logout
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Logout_RevokedRefreshToken_CannotBeUsedToRefresh()
    {
        //Arrange
        var user = DataSeeder.GetApplicationUser();
        var auth = await AuthenticateAsync(user.Email, DataSeeder.DefaultPassword);

        //Act - logout revokes the presented refresh token
        var logout = await HttpClient.PostAsJsonAsync("auth/logout",
            new LogoutRequestDto { RefreshToken = auth.RefreshToken });

        //Assert
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
        var refresh = await HttpClient.PostAsJsonAsync("auth/refresh",
            new RefreshRequestDto { RefreshToken = auth.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }
}