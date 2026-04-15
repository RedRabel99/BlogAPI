using BlogAPI.Application.DTOs.UserProfile;
using BlogAPI.Application.DTOs.UserProfiles;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Interfaces
{
    public interface IUserProfileService
    {
        Task<Result<UserProfileDto>> GetUserProfileById(Guid id);
        Task<Result<UserProfileDto>> GetUserProfileByUsername(string username);
        Task<Result<UserProfileDto>> GetCurrentUserProfileAsync();
        Task<Result<PagedList<UserProfileDto>>> GetUserProfiles(UserProfileQueryParametersDto queryParameters);
        Task<Result> DeleteUserProfileById(Guid id);
        Task<Result<UserProfileDto>> UpdateUserProfile(Guid id, UpdateUserProfileDto userProfile);
    }
}
