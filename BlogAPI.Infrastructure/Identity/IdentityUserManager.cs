using BlogAPI.Application.DTOs.Auth;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Auth;
using Microsoft.AspNetCore.Identity;

namespace BlogAPI.Infrastructure.Identity;

public class IdentityUserManager : IUserManager
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _appDbContext;

    public IdentityUserManager(UserManager<ApplicationUser> userManager, AppDbContext appDbContext)
    {
        _userManager = userManager;
        _appDbContext = appDbContext;
    }

    public async Task<Result> CreateUserAsync(RegisterDto registerUserDto)
    {
        try
        {
            await _appDbContext.Database.BeginTransactionAsync();

            var user = new ApplicationUser
            {
                UserName = registerUserDto.Username,
                Email = registerUserDto.Email
            };

            var result = await _userManager.CreateAsync(user, registerUserDto.Password);

            if (result is null)
            {
                return Result.Failure(AuthErrors.AuthFailure);
            }

            if (result.Succeeded is false)
            {
                if (result.Errors.Any(x => x.Code.Contains("DuplicateUserName")))
                {
                    return Result.Failure(AuthErrors.UserAlreadyExists);
                }

                if (result.Errors.Any(x => x.Code.Equals("DuplicateEmail")))
                {
                    return Result.Failure(AuthErrors.UserAlreadyExists);
                }

                return Result.Failure(AuthErrors.AuthFailure);
            }

            var userProfile = new UserProfile
            {
                Username = user.UserName,
                DisplayName = registerUserDto.DisplayName,
                ApplicationUserId = user.Id
            };

            await _appDbContext.UserProfiles.AddAsync(userProfile);
            await _appDbContext.SaveChangesAsync();

            await _appDbContext.Database.CommitTransactionAsync();
            return Result.Success();
        }
        catch
        {
            await _appDbContext.Database.RollbackTransactionAsync();
            throw;
        }
}

    public async Task<Result<IUserInfo>> FindByEmailAsync(string email)
    {
        var user =  await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return Result<IUserInfo>.Failure(AuthErrors.UserNotFound);
        }

        return Result<IUserInfo>.Success(new UserInfoAdapter(user));
    }

    public async Task<Result<IUserInfo>> FindByIdAsync(string userId)
    {   
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Result<IUserInfo>.Failure(AuthErrors.UserNotFound);
        }


        return Result<IUserInfo>.Success(new UserInfoAdapter(user));
    }

    public async Task<Result<IUserInfo>> ValidateUserAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if(user is null)
        {
            return Result<IUserInfo>.Failure(AuthErrors.InvalidCredentials);
        }

        var isValid = await _userManager.CheckPasswordAsync(user, password);

        if (isValid is not true)
        {
            return Result<IUserInfo>.Failure(AuthErrors.InvalidCredentials);
        }
        return Result<IUserInfo>.Success(new UserInfoAdapter(user));
    }

    public async Task RemoveByIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return;
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return;
        await _userManager.DeleteAsync(user);
    }

    public async Task<Result> ChangeUsernameAsync(string userId, string username)
    {
        try
        {
            await _appDbContext.Database.BeginTransactionAsync();
            var applicationUser = await _userManager.FindByIdAsync(userId);
            if (applicationUser is null)
            {
                return Result.Failure(AuthErrors.UserNotFound);
            }

            applicationUser.UserName = username;
            var updateResult = await _userManager.UpdateAsync(applicationUser);

            if (!updateResult.Succeeded)
            {
                var isDuplicateUsername = updateResult.Errors
                    .Any(x => x.Code.Equals("DuplicateUserName"));
                return Result.Failure(isDuplicateUsername
                    ? AuthErrors.UserAlreadyExists
                    : AuthErrors.Internal);
            }

            var userProfile = _appDbContext.UserProfiles
                .FirstOrDefault(x => x.ApplicationUserId.Equals(applicationUser.Id));

            if (userProfile is null)
            {
                return Result.Failure(UserProfileErrors.NotFound);
            }

            userProfile.Username = username;
            _appDbContext.Update(userProfile);
            await _appDbContext.SaveChangesAsync();
            await _appDbContext.Database.CommitTransactionAsync();

            return Result.Success();
        }
        catch
        {
            await _appDbContext.Database.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<Result<string>> GenerateChangeEmailTokenAsync(string userId, string newEmail)
{
        if (string.IsNullOrEmpty(userId))
        {
            return Result<string>.Failure(AuthErrors.UserNotFound);
        }

        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser is null)
        {
            return Result<string>.Failure(AuthErrors.UserNotFound);
        }

        var token = await _userManager.GenerateChangeEmailTokenAsync(appUser, newEmail);

        return Result<string>.Success(token);
}

    public async Task<Result> ChangeEmailAsync(string userId, string newEmail, string token)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        var updateResult = await _userManager.ChangeEmailAsync(appUser, newEmail, token);

        if (!updateResult.Succeeded)
        {
            if(updateResult.Errors.Any(x => x.Code.Equals("DuplicateEmail")))
            {
                return Result.Failure(AuthErrors.UserWithEmailAlreadyExists);
            }
            if(updateResult.Errors.Any(x => x.Code.Equals("InvalidToken")))
            {
                return Result.Failure(AuthErrors.InvalidToken);
            }
            
            return Result.Failure(AuthErrors.Internal);
        }

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        var appUser = await _userManager.FindByIdAsync(userId);

        if (appUser is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        var result = await _userManager.ChangePasswordAsync(appUser, oldPassword, newPassword);

        if (!result.Succeeded)
        {
            //TODO: implement result for invalid new password
            return Result.Failure(
                result.Errors.Any(x => x.Code.Equals("PasswordMismatch"))
                    ? AuthErrors.PasswordMissmatch 
                    : AuthErrors.Internal);
        }

        return Result.Success();
    }
}
