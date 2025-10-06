using BlogAPI.Application.DTOs;
using BlogAPI.Application.Shared;
using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Interfaces
{
    public interface IUserProfileService
    {
        public Task<Result<UserProfileDto>> GetUserProfileById(Guid id);
        public Task<Result<PagedList<UserProfileDto>>> GetUserProfiles(UserProfileQueryParameters queryParameters);
        public Task<Result> DeleteUserProfileById(Guid id);
        public Task<Result<UserProfileDto>> UpdateUserProfile(Guid id, UpdateUserProfileDto userProfile);
    }
}
