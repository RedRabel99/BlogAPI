using BlogAPI.Application.Auth.Dtos;
using BlogAPI.Application.Common.Errors;
using BlogAPI.Application.Common.Persistance;
using BlogAPI.Application.Auth;
using BlogAPI.Application.Common.Email;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Interfaces.Auth;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;

namespace BlogAPI.Application.UnitTests.Services;

public class AuthServiceTests
{
    private readonly IUserManager _userManager;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserContext _userContext;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<GenerateChangeEmailTokenDto> _generateChangeEmailTokenValidator;
    private readonly IValidator<ChangeEmailDto> _changeEmailValidator;
    private readonly IValidator<ChangeUsernameDto> _changeUsernameValidator;
    private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
    private readonly IValidator<ResendConfirmationEmailDto> _resendConfirmationEmailValidator;
    private readonly IValidator<ForgotPasswordDto> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordDto> _resetPasswordValidator;
    private readonly IValidator<ConfirmEmailDto> _confirmEmailValidator;
    private readonly IValidator<RefreshRequestDto> _refreshRequestValidator;
    private readonly IValidator<LogoutRequestDto> _logoutRequestValidator;
    private readonly IEmailQueue _emailQueue;
    private readonly IAppDbContext _appDbContext;

    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userManager = Substitute.For<IUserManager>();
        _accessTokenService = Substitute.For<IAccessTokenService>();
        _refreshTokenService = Substitute.For<IRefreshTokenService>();
        _userContext = Substitute.For<IUserContext>();
        _registerValidator = Substitute.For<IValidator<RegisterDto>>();
        _loginValidator = Substitute.For<IValidator<LoginDto>>();
        _generateChangeEmailTokenValidator = Substitute.For<IValidator<GenerateChangeEmailTokenDto>>();
        _changeEmailValidator = Substitute.For<IValidator<ChangeEmailDto>>();
        _changeUsernameValidator = Substitute.For<IValidator<ChangeUsernameDto>>();
        _changePasswordValidator = Substitute.For<IValidator<ChangePasswordDto>>();
        _resendConfirmationEmailValidator = Substitute.For<IValidator<ResendConfirmationEmailDto>>();
        _forgotPasswordValidator = Substitute.For<IValidator<ForgotPasswordDto>>();
        _resetPasswordValidator = Substitute.For<IValidator<ResetPasswordDto>>();
        _confirmEmailValidator = Substitute.For<IValidator<ConfirmEmailDto>>();
        _refreshRequestValidator = Substitute.For<IValidator<RefreshRequestDto>>();
        _logoutRequestValidator = Substitute.For<IValidator<LogoutRequestDto>>();
        _emailQueue = Substitute.For<IEmailQueue>();
        _appDbContext = Substitute.For<IAppDbContext>();

        // Validators pass by default; override per-test to exercise validation-failure paths.
        _refreshRequestValidator.Validate(Arg.Any<RefreshRequestDto>()).Returns(new ValidationResult());
        _logoutRequestValidator.Validate(Arg.Any<LogoutRequestDto>()).Returns(new ValidationResult());

        _sut = new AuthService(
            _userManager,
            _accessTokenService,
            _refreshTokenService,
            _userContext,
            _registerValidator,
            _loginValidator,
            _generateChangeEmailTokenValidator,
            _changeEmailValidator,
            _changeUsernameValidator,
            _changePasswordValidator,
            _resendConfirmationEmailValidator,
            _forgotPasswordValidator,
            _resetPasswordValidator,
            _refreshRequestValidator,
            _logoutRequestValidator,
            _emailQueue,
            _confirmEmailValidator,
            _appDbContext);
    }

    // ----- RefreshTokenAsync -----

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsRotatedTokens()
    {
        //Arrange
        var user = Substitute.For<IUserInfo>();

        _refreshTokenService.RotateAsync("old-refresh", Arg.Any<CancellationToken>())
            .Returns(Result<RefreshRotationResult>.Success(
                new RefreshRotationResult("new-refresh", "user-1")));
        _userManager.FindByIdAsync("user-1")
            .Returns(Result<IUserInfo>.Success(user));
        _accessTokenService.GenerateAccessTokenAsync(user)
            .Returns(new AccessTokenResult("access-jwt", 900));

        //Act
        var result = await _sut.RefreshTokenAsync(new RefreshRequestDto { RefreshToken = "old-refresh" });

        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("access-jwt", result.Value.AccessToken);
        Assert.Equal("new-refresh", result.Value.RefreshToken);
        Assert.Equal(900, result.Value.ExpiresInSeconds);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenRotationFails_ReturnsErrorAndDoesNotIssueAccessToken()
    {
        //Arrange
        _refreshTokenService.RotateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<RefreshRotationResult>.Failure(RefreshTokenErrors.Invalid));

        //Act
        var result = await _sut.RefreshTokenAsync(new RefreshRequestDto { RefreshToken = "bad" });

        //Assert
        Assert.True(result.IsError);
        Assert.Equal(RefreshTokenErrors.Invalid, result.Error);
        await _accessTokenService.DidNotReceive().GenerateAccessTokenAsync(Arg.Any<IUserInfo>());
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenUserNotFound_ReturnsErrorAndDoesNotIssueAccessToken()
    {
        //Arrange
        _refreshTokenService.RotateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<RefreshRotationResult>.Success(
                new RefreshRotationResult("new-refresh", "id")));
        _userManager.FindByIdAsync("id")
            .Returns(Result<IUserInfo>.Failure(AuthErrors.UserNotFound));

        //Act
        var result = await _sut.RefreshTokenAsync(new RefreshRequestDto { RefreshToken = "old-refresh" });

        //Assert
        Assert.True(result.IsError);
        Assert.Equal(AuthErrors.UserNotFound, result.Error);
        await _accessTokenService.DidNotReceive().GenerateAccessTokenAsync(Arg.Any<IUserInfo>());
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenValidationFails_ReturnsErrorAndDoesNotRotate()
    {
        //Arrange
        _refreshRequestValidator.Validate(Arg.Any<RefreshRequestDto>())
            .Returns(new ValidationResult(
            [
                new ValidationFailure(nameof(RefreshRequestDto.RefreshToken), "Refresh token is required.")
            ]));

        //Act
        var result = await _sut.RefreshTokenAsync(new RefreshRequestDto { RefreshToken = "" });

        //Assert
        Assert.True(result.IsError);
        await _refreshTokenService.DidNotReceive().RotateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _accessTokenService.DidNotReceive().GenerateAccessTokenAsync(Arg.Any<IUserInfo>());
    }

    // ----- LogoutAsync -----

    [Fact]
    public async Task LogoutAsync_RevokesPresentedRefreshToken_ReturnsSuccess()
    {
        //Arrange

        //Act
        var result = await _sut.LogoutAsync(new LogoutRequestDto { RefreshToken = "refresh-token" });

        //Assert
        Assert.True(result.IsSuccess);
        await _refreshTokenService.Received(1).RevokeAsync("refresh-token", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAsync_WhenValidationFails_ReturnsErrorAndDoesNotRevoke()
    {
        //Arrange
        _logoutRequestValidator.Validate(Arg.Any<LogoutRequestDto>())
            .Returns(new ValidationResult(
            [
                new ValidationFailure(nameof(LogoutRequestDto.RefreshToken), "Refresh token is required.")
            ]));

        //Act
        var result = await _sut.LogoutAsync(new LogoutRequestDto { RefreshToken = "" });

        //Assert
        Assert.True(result.IsError);
        await _refreshTokenService.DidNotReceive().RevokeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
