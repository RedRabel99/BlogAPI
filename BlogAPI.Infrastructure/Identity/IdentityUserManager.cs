using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Interfaces.Auth;
using Microsoft.AspNetCore.Identity;

namespace BlogAPI.Infrastructure.Identity;

public class IdentityUserManager : IUserManager
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityUserManager(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<IUserInfo>> CreateUserAsync(string email, string password)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email
        };
        var result = await _userManager.CreateAsync(user, password);
        if(result is null)
        {
            return Result<IUserInfo>.Failure(AuthErrors.AuthFailure);
        }
        if(result.Succeeded is false)
        {
            //TODO: implement corect auth errors
            return Result<IUserInfo>.Failure(AuthErrors.AuthFailure);
        }

        return Result<IUserInfo>.Success(new UserInfoAdapter(user));
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
}
