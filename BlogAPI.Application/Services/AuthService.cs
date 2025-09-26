using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Interfaces;
using BlogAPI.Domain.Models;

namespace BlogAPI.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserManager _userManager;
    private readonly ITokenService _tokenService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserContext _userContext;

    public AuthService(
        IUserManager userManager,
        ITokenService tokenService,
        IUserProfileRepository userProfileRepository,
        IUserContext userContext)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _userProfileRepository = userProfileRepository;
        _userContext = userContext;
    }

    public async Task<Result<UserProfileDto>> GetCurrentUserProfileAsync(string userId)
    {
        //rewrite is authenticated logic 
        if (!_userContext.IsAuthenticated)
        {
            return null;
        }
        var userProfile = await _userProfileRepository
            .GetByIdentityUserIdAsync(_userContext.UserId);
        if(userProfile == null)
        {
            return null;
        }
        return Result<UserProfileDto>.Success(MapToDto(userProfile));
    }

    public async Task<Result<string>>LoginAsync(LoginDto loginDto)
    {
        var authResult = await _userManager
            .ValidateUserAsync(loginDto.Email, loginDto.Password);
        if (authResult.IsSuccess is false)
        {
            return Result<string>.Failure(authResult.Error);
        }
        var token = await _tokenService.GenerateTokenAsync(authResult.Value);

        return Result<string>.Success(token);
    }

    public async Task<Result<string>> RegisterAsync(RegisterDto registerDto)
    {
        var authResult = await _userManager
            .CreateUserAsync(registerDto.Email, registerDto.Password);

        if (authResult.IsError is true)
        {
            return Result<string>.Failure(authResult.Error);
        }

        var userProfile = new UserProfile()
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = authResult.Value.Id,
            UserName = registerDto.UserName,
            DisplayName = registerDto.DisplayName
        };
        await _userProfileRepository.CreateAsync(userProfile);

        var token = await _tokenService.GenerateTokenAsync(authResult.Value);

        return Result<string>.Success(token);
    }

    private UserProfileDto MapToDto(UserProfile userProfile)
    {
        return new UserProfileDto
        {
            Id = userProfile.Id,
            IdentityUserId = userProfile.ApplicationUserId,
            UserName = userProfile.UserName,
            DisplayName = userProfile.DisplayName,
        };
    }
}
