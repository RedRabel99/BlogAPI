using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain;
using FluentValidation;
using BlogAPI.Application.Extensions;
using BlogAPI.Application.DTOs.Auth;
using BlogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BlogAPI.Application.Interfaces.Auth;

namespace BlogAPI.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserManager _userManager;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserContext _userContext;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<ChangeUsernameDto> _changeUsernameValidator;
    private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
    private readonly IValidator<GenerateChangeEmailTokenDto> _generateChangeEmailTokenDtoValidator;
    private readonly IValidator<ChangeEmailDto> _changeEmailDtoValidator;
    private readonly IValidator<ConfirmEmailDto> _confirmEmailValidator;
    private readonly IValidator<ResendConfirmationEmailDto> _resendConfirmationEmailValidator;
    private readonly IValidator<ForgotPasswordDto> _forgotPasswordValidator;
    private readonly IValidator<ResetPasswordDto> _resetPasswordValidator;
    private readonly IEmailQueue _emailQueue;
    private readonly IAppDbContext _appDbContext;

    public AuthService(
        IUserManager userManager,
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService,
        IUserContext userContext,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        IValidator<GenerateChangeEmailTokenDto> generateChangeEmailTokenDtoValidator,
        IValidator<ChangeEmailDto> changeEmailDtoValidator,
        IValidator<ChangeUsernameDto> changeUsernameValidator,
        IValidator<ChangePasswordDto> changePasswordValidator,
        IValidator<ResendConfirmationEmailDto> resendConfirmationEmailValidator,
        IValidator<ForgotPasswordDto> forgotPasswordValidator,
        IValidator<ResetPasswordDto> resetPasswordValidator,
        IEmailQueue emailQueue,
        IValidator<ConfirmEmailDto> confirmEmailValidator,
        IAppDbContext appDbContext)
    {
        _userManager = userManager;
        _accessTokenService = accessTokenService;
        _refreshTokenService = refreshTokenService;
        _userContext = userContext;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _generateChangeEmailTokenDtoValidator = generateChangeEmailTokenDtoValidator;
        _changeEmailDtoValidator = changeEmailDtoValidator;
        _changeUsernameValidator = changeUsernameValidator;
        _changePasswordValidator = changePasswordValidator;
        _emailQueue = emailQueue;
        _resendConfirmationEmailValidator = resendConfirmationEmailValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _confirmEmailValidator = confirmEmailValidator;
        _appDbContext = appDbContext;
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken ct = default)
    {
        var validationResult = _loginValidator.Validate(loginDto);

        if (validationResult.IsValid is false)
        {
            return validationResult.ToValidationFailure<AuthResponseDto>();
        }

        var authResult = await _userManager
            .ValidateUserAsync(loginDto.Email, loginDto.Password);
        if (authResult.IsError is true)
        {
            return Result<AuthResponseDto>.Failure(authResult.Error);
        }


        var accessTokenResult = await _accessTokenService.GenerateAccessTokenAsync(authResult.Value);
        var refreshToken = await _refreshTokenService.IssueAsync(authResult.Value.Id, ct);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = accessTokenResult.AccessToken,
            ExpiresInSeconds = accessTokenResult.ExpiresInSeconds,
            RefreshToken = refreshToken
        });
    }

    public async Task<Result> RegisterAsync(RegisterDto registerDto, CancellationToken ct = default)
    {
        var validationResult = _registerValidator.Validate(registerDto);

        if (validationResult.IsValid is false)
        {
            return validationResult.ToValidationFailure<string>();
        }

        await using var transaction = await _appDbContext.BeginTransactionAsync(ct); //rollback on dispose if not commited

        var createResult = await _userManager.CreateUserAsync(registerDto.Username, registerDto.Email, registerDto.Password);
        if (createResult.IsError)
        {
            return createResult;
        }

        var userId = createResult.Value;

        _appDbContext.UserProfiles.Add(new UserProfile
        {
            ApplicationUserId = userId,
            Username = registerDto.Username,
            DisplayName = registerDto.DisplayName,
        });

        var token = await _userManager.GenerateConfirmationTokenAsync(registerDto.Email);
        if (token.IsError)
        {
            return Result.Failure(token.Error);
        }

        await _emailQueue.EnqueueToOutbox(new EmailMessage(
            To: registerDto.Email,
            Subject: "Confirm your email",
            Body: $"Please confirm your email with token = {token.Value}",
            IsHtml: false
            ));
      
        await _appDbContext.SaveChangesAsync(); //fix possible unique constraint violation rare case later
        await transaction.CommitAsync();
        
        return Result.Success();
    }

    public async Task<Result> ChangeUsernameAsync(ChangeUsernameDto changeUsernameDto, CancellationToken ct = default)
    {
        var appUserId = _userContext.UserId;
        if (string.IsNullOrEmpty(appUserId))
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        var validation = _changeUsernameValidator.Validate(changeUsernameDto);
        if (!validation.IsValid)
        {
            return validation.ToValidationFailure();
        }
        await using var tx = await _appDbContext.BeginTransactionAsync();

        var updateResult = await _userManager.UpdateUsernameAsync(appUserId, changeUsernameDto.Username);
        if (updateResult.IsError)
        {
            return updateResult;
        }

        var profile = await _appDbContext.UserProfiles.FirstOrDefaultAsync(p => p.ApplicationUserId == appUserId);

        if (profile is null)
        {
            return Result.Failure(UserProfileErrors.NotFound);
        }

        profile.Username = changeUsernameDto.Username;

        await _appDbContext.SaveChangesAsync();
        await tx.CommitAsync();

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordDto changePasswordDto, CancellationToken ct = default)
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

    public async Task<Result> GenerateChangeEmailTokenAsync(GenerateChangeEmailTokenDto generateChangeEmailTokenDto, CancellationToken ct = default)
    {
        var appUserId = _userContext.UserId;
        if (string.IsNullOrEmpty(appUserId))
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        var validationResult = _generateChangeEmailTokenDtoValidator.Validate(generateChangeEmailTokenDto);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure();
        }

        var result = await _userManager.GenerateChangeEmailTokenAsync(appUserId, generateChangeEmailTokenDto.Email);
        if (result.IsError)
        {
            return Result.Failure(result.Error);
        }

        //create email message template later
        await _emailQueue.EnqueueToOutbox(new EmailMessage(
            To: generateChangeEmailTokenDto.Email,
            Subject: "Confirm your email change",
            Body: $"Please confirm your email change with token = {result.Value}",
            IsHtml: false
        ));

        await _appDbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ChangeEmailAsync(ChangeEmailDto changeEmailDto, CancellationToken ct = default)
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

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto, CancellationToken ct = default)
    {
        var validationResult = _confirmEmailValidator.Validate(confirmEmailDto);

        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure();
        }

        return await _userManager.ConfirmEmailAsync(confirmEmailDto.Email, confirmEmailDto.Token);
    }

    public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailDto resendConfirmationEmailDto, CancellationToken ct = default)
    {
        var validationResult = _resendConfirmationEmailValidator.Validate(resendConfirmationEmailDto);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure();
        }

        var tokenResult = await _userManager.GenerateConfirmationTokenAsync(resendConfirmationEmailDto.Email);
        if(tokenResult.IsError)
        {
            return Result.Success(); //return success even if token was not generated, to prevent email enumeration
        }

        var token = tokenResult.Value;

        await _emailQueue.EnqueueToOutbox(new EmailMessage(
            To: resendConfirmationEmailDto.Email,
            Subject: "Confirm your email",
            Body: $"Please confirm your email with token = {token}",
            IsHtml: false
        ));
        
        await _appDbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto, CancellationToken ct = default)
    {
        var validationResult = _forgotPasswordValidator.Validate(forgotPasswordDto);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure();
        }

        var tokenResult = await _userManager.GeneratePasswordResetTokenAsync(forgotPasswordDto.Email);
        if (tokenResult.IsError)
        {
            return Result.Success(); //return success even if token was not generated, to prevent email enumeration
        }

        await _emailQueue.EnqueueToOutbox(new EmailMessage(
            To: forgotPasswordDto.Email,
            Subject: "Reset your password",
            Body: $"Reset your password with token = {tokenResult.Value}",
            IsHtml: false
        ));

        await _appDbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordDto resetPasswordDto, CancellationToken ct = default)
    {
        var validationResult = _resetPasswordValidator.Validate(resetPasswordDto);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure();
        }

        return await _userManager.ResetPasswordAsync(
            resetPasswordDto.Email,
            resetPasswordDto.Token,
            resetPasswordDto.NewPassword);
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshRequestDto refreshRequestDto, CancellationToken ct = default)
    {
        var rotateResult = await _refreshTokenService.RotateAsync(refreshRequestDto.RefreshToken, ct);
        if (rotateResult.IsError)
        {
            return Result<AuthResponseDto>.Failure(rotateResult.Error);
        }

        var user = await _userManager.FindByIdAsync(rotateResult.Value.ApplicationUserId);
        if (user.IsError)
        {
            return Result<AuthResponseDto>.Failure(user.Error);
        }

        var accessTokenResult = await _accessTokenService.GenerateAccessTokenAsync(user.Value);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            AccessToken = accessTokenResult.AccessToken,
            ExpiresInSeconds = accessTokenResult.ExpiresInSeconds,
            RefreshToken = rotateResult.Value.NewRefreshToken
        });
    }

    public async Task<Result> LogoutAsync(LogoutRequestDto logoutRequestDto, CancellationToken ct = default)
    {
        await _refreshTokenService.RevokeAsync(logoutRequestDto.RefreshToken, ct);
        return Result.Success();
    }
}
