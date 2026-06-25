
using BlogAPI.Application.DTOs.UserProfile;
using BlogAPI.Application.DTOs.UserProfiles;
using BlogAPI.Application.Extensions;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Mapping;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Application.Shared.UserProfile;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Interfaces.Auth;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserContext _userContext;
    private readonly IValidator<UpdateUserProfileDto> _updateValidator;
    private readonly IValidator<UserProfileQueryParametersDto> _queryParametersValidation;
    private readonly IPagedListFactory _pagedListFactory;
    private readonly IAppDbContext _appDbContext;
    public UserProfileService(
        IUserContext userContext,
        IValidator<UpdateUserProfileDto> updateValidation,
        IValidator<UserProfileQueryParametersDto> queryParametersValidation,
        IPagedListFactory pagedListFactory,
        IAppDbContext appDbContext)
    {
        _userContext = userContext;
        _updateValidator = updateValidation;
        _queryParametersValidation = queryParametersValidation;
        _pagedListFactory = pagedListFactory;
        _appDbContext = appDbContext;
    }

    public async Task<Result> DeleteUserProfileById(Guid id)
    {
        var userProfile = await _appDbContext.UserProfiles.FindAsync(id);

        if (userProfile is null)
        {
            return Result.Failure(UserProfileErrors.NotFound);
        }
        if (userProfile.ApplicationUserId != _userContext.UserId)
        {
            return Result.Failure(UserProfileErrors.Forbidden);
        }

        _appDbContext.UserProfiles.Remove(userProfile);
        await _appDbContext.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<UserProfileDto>> GetCurrentUserProfileAsync()
    {
        if (string.IsNullOrEmpty(_userContext.UserId))
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.NotFound);
        }

        var userProfile = await _appDbContext.UserProfiles
            .FirstOrDefaultAsync(up => up.ApplicationUserId == _userContext.UserId);

        if (userProfile is null)
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.NotFound);
        }

        return Result<UserProfileDto>.Success(userProfile.ToDto());
    }

    public async Task<Result<UserProfileDto>> GetUserProfileById(Guid id)
    {
        var userProfile = await _appDbContext.UserProfiles.FirstOrDefaultAsync(up => up.Id == id);

        if (userProfile is null)
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.NotFound);
        }

        return Result<UserProfileDto>.Success(userProfile.ToDto());
    }

    public async Task<Result<UserProfileDto>> GetUserProfileByUsername(string username)
    {
        var userProfile = await _appDbContext.UserProfiles.FirstOrDefaultAsync(up => up.Username == username);

        if (userProfile is null)
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.NotFound);
        }

        return Result<UserProfileDto>.Success(userProfile.ToDto());
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

        var query = _appDbContext.UserProfiles
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

        var entity = await _appDbContext.UserProfiles.FirstOrDefaultAsync(up => up.Id == id);

        if (entity is null)
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.NotFound);
        }

        if (entity.ApplicationUserId != _userContext.UserId)
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.Forbidden);
        }

        entity.DisplayName = profile.DisplayName;

        await _appDbContext.SaveChangesAsync();

        return Result<UserProfileDto>.Success(entity.ToDto());
    }
}
