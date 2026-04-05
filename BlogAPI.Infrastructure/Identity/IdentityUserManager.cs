using BlogAPI.Application.DTOs;
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
        await _appDbContext.Database.BeginTransactionAsync();

        var user = new ApplicationUser
        {
            UserName = registerUserDto.Username,
            Email = registerUserDto.Email
        };

        var result = await _userManager.CreateAsync(user, registerUserDto.Password);

        if(result is null)
        {
            return Result.Failure(AuthErrors.AuthFailure);
        }

        if(result.Succeeded is false)
        {
            if(result.Errors.Any(x => x.Code.Contains("DuplicateUserName")))
            {
                return Result.Failure(AuthErrors.UserAlreadyExists);
            }

            if(result.Errors.Any(x => x.Code.Equals("DuplicateEmail")))
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
        


        return Result<IUserInfo>.Success(new UserInfoAdapter(user));
    }

    public async Task<Result<IUserInfo>> ValidateUserAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if(user is null)
        {
            return Result<IUserInfo>.Failure(AuthErrors.UserNotFound);
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

    public Task<Result> ChangeUsernameAsync(string userid, string username)
    {
        throw new NotImplementedException();
    }

    public Task<Result<string>> GenerateChangeEmailTokenAsync(string userId, string newEmail)
    {
        throw new NotImplementedException();
    }

    public Task<Result> ChangeEmailAsync(string userId, string newEmail, string token)
    {
        throw new NotImplementedException();
    }

    public Task<Result> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        throw new NotImplementedException();
    }
}
