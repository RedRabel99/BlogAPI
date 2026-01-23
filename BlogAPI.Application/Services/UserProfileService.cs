using AutoMapper;
using AutoMapper.QueryableExtensions;
using BlogAPI.Application.DTOs;
using BlogAPI.Application.Extensions;
using BlogAPI.Application.Interfaces;
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
    private readonly IMapper _mapper;   
    private readonly IUserContext _userContext;
    private readonly IValidator<UpdateUserProfileDto> _updateValidator;
    private readonly IValidator<UserProfileQueryParametersDto> _queryParametersValidation;
    private readonly IPagedListFactory _pagedListFactory;

    public UserProfileService(
        IUserProfileRepository userProfileRepository,
        IMapper mapper,
        IUserContext userContext,
        IValidator<UpdateUserProfileDto> updateValidation,
        IValidator<UserProfileQueryParametersDto> queryParametersValidation,
        IPagedListFactory pagedListFactory
        )
    {
        _userProfileRepository = userProfileRepository;
        _mapper = mapper;
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

    public async Task<Result<UserProfileDto>> GetUserProfileById(Guid id)
    {
        var userProfile = await _userProfileRepository.GetByIdAsync(id);
        if (userProfile is null)
        {
            return Result<UserProfileDto>
                .Failure(UserProfileErrors.NotFound);
        }
        var resutUserProfile = _mapper.Map<UserProfileDto>(userProfile);
        return Result<UserProfileDto>.Success(resutUserProfile);
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

            .ProjectTo<UserProfileDto>(_mapper.ConfigurationProvider);

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

        entity.UserName = profile.UserName;
        entity.DisplayName = profile.DisplayName;

        var result = await _userProfileRepository.UpdateAsync(entity);

        if(result is null)
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.Internal);
        }

        return Result<UserProfileDto>.Success(_mapper.Map<UserProfileDto>(result));
    }
}
