using BlogAPI.Application.DTOs.Auth;
using BlogAPI.Application.Services;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.UserProfiles;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace Tests.Application;

public class AuthServiceTests
{
    private readonly IUserManager _userManager;
    private readonly ITokenService _tokenService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserContext _userContext;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<ChangeUsernameDto> _changeUsernameValidator;
    private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
    private readonly IValidator<GenerateChangeEmailTokenDto> _generateChangeEmailTokenDtoValidator;
    private readonly IValidator<ChangeEmailDto> _changeEmailDtoValidator;

    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userManager = Substitute.For<IUserManager>();
        _tokenService = Substitute.For<ITokenService>();
        _userProfileRepository = Substitute.For<IUserProfileRepository>();
        _userContext = Substitute.For<IUserContext>();
        _registerValidator = Substitute.For<IValidator<RegisterDto>>();
        _loginValidator = Substitute.For<IValidator<LoginDto>>();
        _changeUsernameValidator = Substitute.For<IValidator<ChangeUsernameDto>>();
        _changePasswordValidator = Substitute.For<IValidator<ChangePasswordDto>>();
        _generateChangeEmailTokenDtoValidator = Substitute.For<IValidator<GenerateChangeEmailTokenDto>>();
        _changeEmailDtoValidator = Substitute.For<IValidator<ChangeEmailDto>>();

        _sut = new AuthService(
            _userManager,
            _tokenService,
            _userProfileRepository,
            _userContext,
            _registerValidator,
            _loginValidator,
            _generateChangeEmailTokenDtoValidator,
            _changeEmailDtoValidator,
            _changeUsernameValidator,
            _changePasswordValidator);
    }

    [Fact]
    public async Task LoginAsync_WhenValidationFails_ReturnsValidationFailure()
    {
        // Arrange
        var dto = new LoginDto { Email = "notamail", Password = "password" };
        var failures = new[] { new ValidationFailure("Email", "err") };
        var validation = new ValidationResult(failures);
        _loginValidator.Validate(dto).Returns(validation);

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsInvalid_ReturnsFailureFromUserManager()
    {
        // Arrange
        var dto = new LoginDto { Email = "user@mail.com", Password = "p" };
        _loginValidator.Validate(dto).Returns(new ValidationResult());
        _userManager.ValidateUserAsync(dto.Email, dto.Password).Returns(Task.FromResult(Result<IUserInfo>.Failure(AuthErrors.InvalidCredentials)));

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(AuthErrors.InvalidCredentials.Code, result.Error.Code);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var dto = new LoginDto { Email = "user@mail.com", Password = "password" };
        _loginValidator.Validate(dto).Returns(new ValidationResult());
        var userInfo = Substitute.For<IUserInfo>();
        _userManager.ValidateUserAsync(dto.Email, dto.Password).Returns(Task.FromResult(Result<IUserInfo>.Success(userInfo)));
        _tokenService.GenerateTokenAsync(userInfo).Returns(Task.FromResult("token-value"));

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("token-value", result.Value);
    }

    [Fact]
    public async Task RegisterAsync_WhenValidationFails_ReturnsValidationFailure()
    {
        // Arrange
        var dto = new RegisterDto { Email = "notamail" };
        var validation = new ValidationResult(new[] { new ValidationFailure("Email", "err") });
        _registerValidator.Validate(dto).Returns(validation);

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameAlreadyExists_ReturnsError()
    {
        // Arrange
        var dto = new RegisterDto { Username = "existing-username",Email = "user@mail.com" };
        _registerValidator.Validate(dto).Returns(new ValidationResult());
        _userManager.CreateUserAsync(dto).Returns(Task.FromResult(Result.Failure(AuthErrors.UserAlreadyExists)));

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(AuthErrors.UserAlreadyExists.Code, result.Error.Code);
    }

    [Fact]
    public async Task RegisterAsync_WhenSuccessful_ReturnsSuccess()
    {
        // Arrange
        var dto = new RegisterDto { Username = "username", Email = "user@mail.com", Password ="password" };
        _registerValidator.Validate(dto).Returns(new ValidationResult());
        _userManager.CreateUserAsync(dto).Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ChangeUsernameAsync_WithoutAuthenticatedUser_ReturnsUserNotFound()
    {
        // Arrange
        _userContext.UserId.Returns((string?)null);

        // Act
        var result = await _sut.ChangeUsernameAsync(new ChangeUsernameDto { Username = "new-username" });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(AuthErrors.UserNotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task ChangeUsernameAsync_WithInvalidUsername_ReturnsValidationFailure()
    {
        // Arrange
        _userContext.UserId.Returns("id");
        var dto = new ChangeUsernameDto { Username = "invalid-username" };
        _changeUsernameValidator.Validate(dto).Returns(new ValidationResult(new[] { new ValidationFailure("Username", "err") }));

        // Act
        var result = await _sut.ChangeUsernameAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task ChangeUsernameAsync_WhenUserWithUsernameExists_ReturnsError()
    {
        // Arrange
        _userContext.UserId.Returns("id");
        var dto = new ChangeUsernameDto { Username = "existing-username" };
        _changeUsernameValidator.Validate(dto).Returns(new ValidationResult());
        _userManager.ChangeUsernameAsync("id", dto.Username).Returns(Task.FromResult(Result.Failure(AuthErrors.UserAlreadyExists)));

        // Act
        var result = await _sut.ChangeUsernameAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(AuthErrors.UserAlreadyExists.Code, result.Error.Code);
    }

    [Fact]
    public async Task ChangeUsernameAsync_WhenSuccessful_ReturnsSuccess()
    {
        // Arrange
        _userContext.UserId.Returns("id");
        var dto = new ChangeUsernameDto { Username = "new-username" };
        _changeUsernameValidator.Validate(dto).Returns(new ValidationResult());
        _userManager.ChangeUsernameAsync("id", dto.Username).Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _sut.ChangeUsernameAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GenerateChangeEmailTokenAsync_WithNoAuthenticatedUser_ReturnsUserNotFound()
    {
        // Arrange
        _userContext.UserId.Returns((string?)null);

        // Act
        var result = await _sut.GenerateChangeEmailTokenAsync(new GenerateChangeEmailTokenDto { Email = "new@mail.com" });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(AuthErrors.UserNotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GenerateChangeEmailTokenAsync_WithValidEmail_ReturnsTokenDto()
    {
        // Arrange
        _userContext.UserId.Returns("id");
        var dto = new GenerateChangeEmailTokenDto { Email = "new@mail.com" };
        _generateChangeEmailTokenDtoValidator.Validate(dto).Returns(new ValidationResult());
        var token = "token";
        _userManager.GenerateChangeEmailTokenAsync("id", dto.Email).Returns(Task.FromResult(Result<string>.Success(token)));

        // Act
        var result = await _sut.GenerateChangeEmailTokenAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(token, result.Value.Token);
    }

    [Fact]
    public async Task ChangeEmailAsync_WithNoAuthenticatedUser_ReturnsUserNotFound()
    {
        // Arrange
        _userContext.UserId.Returns((string?)null);

        // Act
        var result = await _sut.ChangeEmailAsync(new ChangeEmailDto { Email = "new@mail.com", Token = "token" });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(AuthErrors.UserNotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task ChangeEmailAsync_WhenUserWithMailAlreadyExists_ReturnsError()
    {
        // Arrange
        _userContext.UserId.Returns("id");
        var dto = new ChangeEmailDto { Email = "existing@mail.com", Token = "token" };
        _changeEmailDtoValidator.Validate(dto).Returns(new ValidationResult());
        _userManager.ChangeEmailAsync("id", dto.Email, dto.Token).Returns(Task.FromResult(Result.Failure(AuthErrors.UserWithEmailAlreadyExists)));

        // Act
        var result = await _sut.ChangeEmailAsync(dto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(AuthErrors.UserWithEmailAlreadyExists.Code, result.Error.Code);
    }

    [Fact]
    public async Task ChangeEmailAsync_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var id = "id";
        _userContext.UserId.Returns(id);
        var dto = new ChangeEmailDto { Email = "new@mail.com", Token = "token" };
        _changeEmailDtoValidator.Validate(dto).Returns(new ValidationResult());
        _userManager.ChangeEmailAsync(id, dto.Email, dto.Token).Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _sut.ChangeEmailAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        await _userManager.Received(1).ChangeEmailAsync(id, dto.Email, dto.Token);
    }
}
