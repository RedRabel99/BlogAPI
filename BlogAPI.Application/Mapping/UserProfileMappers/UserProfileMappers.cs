using BlogAPI.Application.DTOs.UserProfile;
using BlogAPI.Domain.Entities;
using System.Linq.Expressions;

namespace BlogAPI.Application.Mapping.UserProfileMappers;

public static class UserProfileMappers
{
    public static UserProfileDto ToDto(this UserProfile userProfile) 
        => new UserProfileDto
        {
            Id = userProfile.Id,
            ApplicationUserId = userProfile.ApplicationUserId,
            UserName = userProfile.Username,
            DisplayName = userProfile.DisplayName
        };

    public static UserProfile ToDomain(this UserProfileDto userProfileDto)
        => new UserProfile
        {
            Id = userProfileDto.Id,
            ApplicationUserId = userProfileDto.ApplicationUserId,
            Username = userProfileDto.UserName,
            DisplayName = userProfileDto.DisplayName
        };

    public static Expression<Func<UserProfile, UserProfileDto>> ProjectToDto =>
        userProfile => new UserProfileDto
        {
            Id = userProfile.Id,
            ApplicationUserId = userProfile.ApplicationUserId,
            UserName = userProfile.Username,
            DisplayName = userProfile.DisplayName
        };
}
