using BlogAPI.Application.DTOs.UserProfile;
using BlogAPI.Domain.Entities;

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
            DisplayName = userProfileDto.DisplayName
        };
}
