
using BlogAPI.Application.DTOs.UserProfile;
using BlogAPI.Application.DTOs.UserProfiles;
using BlogAPI.Application.Extensions;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Mapping.UserProfileMappers;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Application.Shared.UserProfile;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.UserProfiles;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace BlogAPI.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserContext _userContext;
    private readonly IValidator<UpdateUserProfileDto> _updateValidator;
    private readonly IValidator<UserProfileQueryParametersDto> _queryParametersValidation;
    private readonly IPagedListFactory _pagedListFactory;

    public UserProfileService(
        IUserProfileRepository userProfileRepository,
        IUserContext userContext,
        IValidator<UpdateUserProfileDto> updateValidation,
        IValidator<UserProfileQueryParametersDto> queryParametersValidation,
        IPagedListFactory pagedListFactory
        )
    {
        _userProfileRepository = userProfileRepository;
        _userContext = userContext;
        _updateValidator = updateValidation;
        _queryParametersValidation = queryParametersValidation;
        _pagedListFactory = pagedListFactory;
    }

    public async Task<Result> DeleteUserProfileById(Guid id)
    {
        var userProfile = await _userProfileRepository.GetByIdAsync(id);
        if (userProfile is null)
        {
            return Result.Failure(UserProfileErrors.NotFound);
        }
        if (userProfile.ApplicationUserId != _userContext.UserId)
        {
            return Result.Failure(UserProfileErrors.Forbidden);
        }

        await _userProfileRepository.DeleteAsync(id);

        return Result.Success();
    }

    public async Task<Result<UserProfileDto>> GetCurrentUserProfileAsync()
    {
        if (string.IsNullOrEmpty(_userContext.UserId))
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.NotFound);
        }

        var userProfile = await _userProfileRepository.GetByApplicationUserIdAsync(_userContext.UserId);
        var userProfileDto = userProfile.ToDto();

        return Result<UserProfileDto>.Success(userProfileDto);
    }

    public async Task<Result<UserProfileDto>> GetUserProfileById(Guid id)
    {
        var userProfile = await _userProfileRepository.GetByIdAsync(id);
        if (userProfile is null)
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.NotFound);
        }
        var resutUserProfile = userProfile.ToDto();
        return Result<UserProfileDto>.Success(resutUserProfile);
    }

    public async Task<Result<UserProfileDto>> GetUserProfileByUsername(string username)
    {
        var userProfile = await _userProfileRepository.GetByUsername(username);
        if(userProfile is null)
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.NotFound);
        }
        var resultUserProfile = userProfile.ToDto();
        return Result<UserProfileDto>.Success(resultUserProfile);
    }

    public async Task<Result<PagedList<UserProfileDto>>> GetUserProfiles(UserProfileQueryParametersDto queryParameters)
    {
        var validationResult = _queryParametersValidation.Validate(queryParameters);

        if (validationResult.IsValid is false)
        {
            return validationResult.ToValidationFailure<PagedList<UserProfileDto>>();
        }

        var queryFilters = new UserProfileQueryFiltering(queryParameters.UserName, queryParameters.DisplayName);
        var sortingParams = new UserProfileQuerySorting(queryParameters.SortingOrder, queryParameters.SortColumn);
        
        var query = _userProfileRepository
            .GetAll()
            .ApplyFiltering(queryFilters)
            .ApplySorting(sortingParams)
            .Select(UserProfileMappers.ProjectToDto);

        var result = await _pagedListFactory.CreateAsync(query, queryParameters.Page, queryParameters.PageSize);
        
        return Result<PagedList<UserProfileDto>>.Success(result);
    }

    public async Task<Result<UserProfileDto>> UpdateUserProfile(Guid id, UpdateUserProfileDto profile)
    {
        var validationResult = _updateValidator.Validate(profile);

        if (validationResult.IsValid is false)
        {
            return validationResult.ToValidationFailure<UserProfileDto>();
        }

        var entity = await _userProfileRepository.GetByIdAsync(id);

        if(entity is null)
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.NotFound);
        }

        if(entity.ApplicationUserId != _userContext.UserId)
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.Forbidden);
        }

        entity.DisplayName = profile.DisplayName;

        var result = await _userProfileRepository.UpdateAsync(entity);

        if(result is null)
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.Internal);
        }

        return Result<UserProfileDto>.Success(result.ToDto());
    }
}
