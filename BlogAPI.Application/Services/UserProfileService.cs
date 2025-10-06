using AutoMapper;
using AutoMapper.QueryableExtensions;
using BlogAPI.Application.DTOs;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Shared;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Auth;
using BlogAPI.Domain.Interfaces.UserProfiles;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BlogAPI.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IMapper _mapper;
    private readonly IUserContext _userContext;

    public UserProfileService(
        IUserProfileRepository userProfileRepository,
        IMapper mapper,
        IUserContext userContext)
    {
        _userProfileRepository = userProfileRepository;
        _mapper = mapper;
        _userContext = userContext;
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

    public async Task<Result<PagedList<UserProfileDto>>> GetUserProfiles(UserProfileQueryParameters queryParameters)
    {
        var query = _userProfileRepository.GetAll();

        if (!string.IsNullOrEmpty(queryParameters.UserName))
        {
            query = query.Where(u => u.UserName.Contains(queryParameters.UserName));
        }

        if (!string.IsNullOrEmpty(queryParameters.DisplayName))
        {
            query = query.Where(u => u.DisplayName.Contains(queryParameters.DisplayName));
        }

        Expression<Func<UserProfile, object>> keySelector = queryParameters.SortColumn?.ToLower() switch
        {
            "username" => userProfile => userProfile.UserName,
            "displayname" => userProfile => userProfile.DisplayName,
            _ => userProfile => userProfile.Id
        };
        if (!string.IsNullOrEmpty(queryParameters.SortColumn))
        {
            query = queryParameters.SortingOrder == SortingOrder.Ascending ?
                query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }

        return Result<PagedList<UserProfileDto>>
            .Success(
                await PagedList<UserProfileDto>
                .CreateAsync(
                    query.ProjectTo<UserProfileDto>(_mapper.ConfigurationProvider),
                    queryParameters.Page,
                    queryParameters.PageSize
                    ));
        
    }

    public async Task<Result<UserProfileDto>> UpdateUserProfile(Guid id, UpdateUserProfileDto profile)
    {
        var entity = await _userProfileRepository.GetByIdAsync(id);

        if(entity is null)
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.NotFound);
        }

        if(entity.ApplicationUserId != _userContext.UserId)
        {
            return Result<UserProfileDto>.Failure(UserProfileErrors.Forbidden);
        }
        //var entity = _mapper.Map<UserProfile>(profile);
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
