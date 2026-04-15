using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.UserProfiles;
using BlogAPI.Domain;
using FluentValidation;
using BlogAPI.Application.Extensions;
using BlogAPI.Application.DTOs.Auth;

namespace BlogAPI.Application.Services;

public class AuthService : IAuthService
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

    public AuthService(
        IUserManager userManager,
        ITokenService tokenService,
        IUserProfileRepository userProfileRepository,
        IUserContext userContext,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        IValidator<GenerateChangeEmailTokenDto> generateChangeEmailTokenDtoValidator,
        IValidator<ChangeEmailDto> changeEmailDtoValidator,
        IValidator<ChangeUsernameDto> changeUsernameValidator,
        IValidator<ChangePasswordDto> changePasswordValidator)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _userProfileRepository = userProfileRepository;
        _userContext = userContext;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _generateChangeEmailTokenDtoValidator = generateChangeEmailTokenDtoValidator;
        _changeEmailDtoValidator = changeEmailDtoValidator;
        _changeUsernameValidator = changeUsernameValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    public async Task<Result<string>> LoginAsync(LoginDto loginDto)
    {
        var validationResult = _loginValidator.Validate(loginDto);

        if (validationResult.IsValid is false)
        {
            return validationResult.ToValidationFailure<string>();
        }

        var authResult = await _userManager
            .ValidateUserAsync(loginDto.Email, loginDto.Password);
        if (authResult.IsError is true)
        {
            return Result<string>.Failure(authResult.Error);
        }
        var token = await _tokenService.GenerateTokenAsync(authResult.Value);

        return Result<string>.Success(token);
    }

    public async Task<Result> RegisterAsync(RegisterDto registerDto)
    {
        var validationResult = _registerValidator.Validate(registerDto);

        if (validationResult.IsValid is false)
        {
            return validationResult.ToValidationFailure<string>();
        }

        var authResult = await _userManager
            .CreateUserAsync(registerDto);

        if (authResult.IsError)
        {
            return authResult;
        }

        return Result.Success();
    }

    public async Task<Result> ChangeUsernameAsync(ChangeUsernameDto changeUsernameDto)
    {
        var appUserId = _userContext.UserId;
        if (string.IsNullOrEmpty(appUserId))
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        var validationResult = _changeUsernameValidator.Validate(changeUsernameDto);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure();
        }

        var result = await _userManager.ChangeUsernameAsync(appUserId, changeUsernameDto.Username);

            if (result.IsError)
        {
            return result;
        }

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        var appUserId = _userContext.UserId;
        if (string.IsNullOrEmpty(appUserId))
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        var validationResult = _changePasswordValidator.Validate(changePasswordDto);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure();
        }

        var result = await _userManager.ChangePasswordAsync(appUserId, changePasswordDto.OldPassword, changePasswordDto.NewPassword);

        if (result.IsError)
        {
            return result;
        }

        return Result.Success();
    }

    public async Task<Result<ChangeEmailTokenResponseDto>> GenerateChangeEmailTokenAsync(GenerateChangeEmailTokenDto generateChangeEmailTokenDto)
    {
        var appUserId = _userContext.UserId;
        if (string.IsNullOrEmpty(appUserId))
        {
            return Result<ChangeEmailTokenResponseDto>.Failure(AuthErrors.UserNotFound);
        }

        var validationResult = _generateChangeEmailTokenDtoValidator.Validate(generateChangeEmailTokenDto);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure<ChangeEmailTokenResponseDto>();
        }

        var result = await _userManager.GenerateChangeEmailTokenAsync(appUserId, generateChangeEmailTokenDto.Email);
        if (result.IsError)
        {
            return Result<ChangeEmailTokenResponseDto>.Failure(result.Error);
        }

        return Result<ChangeEmailTokenResponseDto>.Success(new ChangeEmailTokenResponseDto
        {
            Token = result.Value
        });
    }

    public async Task<Result> ChangeEmailAsync(ChangeEmailDto changeEmailDto)
    {
        var appUserId = _userContext.UserId;
        if (string.IsNullOrEmpty(appUserId))
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        var validationResult = _changeEmailDtoValidator.Validate(changeEmailDto);

        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure();
        }

        var result = await _userManager.ChangeEmailAsync(appUserId, changeEmailDto.Email, changeEmailDto.Token);

        return result;
    }
}
