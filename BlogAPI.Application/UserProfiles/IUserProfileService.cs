using BlogAPI.Application.UserProfiles.Dtos;
using BlogAPI.Application.Common.Pagination;
using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.UserProfiles
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
