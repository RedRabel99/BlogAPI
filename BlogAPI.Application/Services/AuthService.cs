using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.UserProfiles;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain;
using AutoMapper;
using FluentValidation;
using BlogAPI.Application.Extensions;
using BlogAPI.Domain.Exceptions;

namespace BlogAPI.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserManager _userManager;
    private readonly ITokenService _tokenService;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserContext _userContext;
    private readonly IMapper _mapper;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;

    public AuthService(
        IUserManager userManager,
        ITokenService tokenService,
        IUserProfileRepository userProfileRepository,
        IUserContext userContext,
        IMapper mapper,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _userProfileRepository = userProfileRepository;
        _userContext = userContext;
        _mapper = mapper;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;

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

    public async Task<Result<Guid>> RegisterAsync(RegisterDto registerDto)
    {
        var validationResult = _registerValidator.Validate(registerDto);

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
            ApplicationUserId = authResult.Value.Id,
            UserName = registerDto.UserName,
            DisplayName = registerDto.DisplayName
        };

        Guid createdProfileId;
        try
        {
            createdProfileId = await _userProfileRepository.CreateAsync(userProfile);
        }
        catch (DuplicateUserNameException)
        {
            await _userManager.RemoveByIdAsync(authResult.Value.Id);
            return Result<Guid>.Failure(UserProfileErrors.UsernameAlreadyExists);
        }catch (Exception)
        {
            await _userManager.RemoveByIdAsync(authResult.Value.Id);
            throw;
        }

        return Result<Guid>.Success(createdProfileId);
    }   
}
