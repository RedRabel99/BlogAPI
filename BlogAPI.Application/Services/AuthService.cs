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
    private readonly IAccessTokenService _tokenService;
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
        IAccessTokenService tokenService,
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
        _tokenService = tokenService;
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
        var token = await _tokenService.GenerateAccessTokenAsync(authResult.Value);

        return Result<string>.Success(token.AccessToken);
    }

    public async Task<Result> RegisterAsync(RegisterDto registerDto)
    {
        var validationResult = _registerValidator.Validate(registerDto);

        if (validationResult.IsValid is false)
        {
            return validationResult.ToValidationFailure<string>();
        }

        await using var transaction = await _appDbContext.BeginTransactionAsync(); //rollback on dispose if not commited

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
            Body: $"Please confirm your email with tokenResult={token.Value}",
            IsHtml: false
            ));
      
        await _appDbContext.SaveChangesAsync(); //fix possible unique constraint violation rare case later
        await transaction.CommitAsync();
        
        return Result.Success();
    }

    public async Task<Result> ChangeUsernameAsync(ChangeUsernameDto changeUsernameDto)
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

    public async Task<Result> GenerateChangeEmailTokenAsync(GenerateChangeEmailTokenDto generateChangeEmailTokenDto)
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
            Body: $"Please confirm your email change with token={result.Value}",
            IsHtml: false
        ));

        await _appDbContext.SaveChangesAsync();

        return Result.Success();
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

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto)
    {
        var validationResult = _confirmEmailValidator.Validate(confirmEmailDto);

        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure();
        }

        return await _userManager.ConfirmEmailAsync(confirmEmailDto.Email, confirmEmailDto.Token);
    }

    public async Task<Result> ResendConfirmationEmailAsync(ResendConfirmationEmailDto resendConfirmationEmailDto)
    {
        var validationResult = _resendConfirmationEmailValidator.Validate(resendConfirmationEmailDto);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure();
        }

        var tokenResult = await _userManager.GenerateConfirmationTokenAsync(resendConfirmationEmailDto.Email);
        if(tokenResult.IsError)
        {
            Result.Success(); //return success even if token was no generated, to prevent email enumeration
        }

        var token = tokenResult.Value;

        await _emailQueue.EnqueueToOutbox(new EmailMessage(
            To: resendConfirmationEmailDto.Email,
            Subject: "Confirm your email",
            Body: $"Please confirm your email with tokenResult={token}",
            IsHtml: false
        ));
        
        await _appDbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
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
            Body: $"Reset your password with token={tokenResult.Value}",
            IsHtml: false
        ));

        await _appDbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
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
}
