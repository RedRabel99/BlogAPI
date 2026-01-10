using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.UserProfiles;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain;
using AutoMapper;
using FluentValidation;
using BlogAPI.Application.Extentions;

namespace BlogAPI.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserManager _userManager;
    private readonly ITokenService _tokenService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserContext _userContext;
    private readonly IMapper _mapper;
    private readonly IValidator<RegisterDto> _validator;

    public AuthService(
        IUserManager userManager,
        ITokenService tokenService,
        IUserProfileRepository userProfileRepository,
        IUserContext userContext,
        IMapper mapper,
        IValidator<RegisterDto> validator)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _userProfileRepository = userProfileRepository;
        _userContext = userContext;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<UserProfileDto>> GetCurrentUserProfileAsync(string userId)
    {
        //rewrite is authenticated logic 
        if (_userContext.IsAuthenticated is false)
        {
            return Result<UserProfileDto>.Failure(AuthErrors.NoAuthenticatedUser);
        }
        var userProfile = await _userProfileRepository
            .GetByApplicationUserIdAsync(_userContext.UserId);

        if(userProfile is null)
        {
            return Result<UserProfileDto>.Failure(AuthErrors.UserNotFound);
        }
        return Result<UserProfileDto>.Success(_mapper.Map<UserProfileDto>(userProfile));
    }

    public async Task<Result<string>>LoginAsync(LoginDto loginDto)
    {
        var authResult = await _userManager
            .ValidateUserAsync(loginDto.Email, loginDto.Password);
        if (authResult.IsError is true)
        {
            return Result<string>.Failure(authResult.Error);
        }
        var token = await _tokenService.GenerateTokenAsync(authResult.Value);

        return Result<string>.Success(token);
    }

    public async Task<Result<Guid>> RegisterAsync(RegisterDto registerDto)
    {
        var validationResult = _validator.Validate(registerDto);

        if (validationResult.IsValid is false)
        {
          return validationResult.ToValidationFailure<Guid>();
        }

        var authResult = await _userManager
            .CreateUserAsync(registerDto.Email, registerDto.Password);

        if (authResult.IsError is true)
        {
            return Result<Guid>.Failure(authResult.Error);
        }

        var userProfile = new UserProfile()
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = authResult.Value.Id,
            UserName = registerDto.UserName,
            DisplayName = registerDto.DisplayName
        };
        //need to check wether it was created
        var id = await _userProfileRepository.CreateAsync(userProfile);

        return Result<Guid>.Success(id);
    }

    private UserProfileDto MapToDto(UserProfile userProfile)
    {
        return new UserProfileDto
        {
            Id = userProfile.Id,
            ApplicationUserId = userProfile.ApplicationUserId,
            UserName = userProfile.UserName,
            DisplayName = userProfile.DisplayName,
        };
    }
}
