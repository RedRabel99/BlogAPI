using BlogAPI.Domain.Models;

namespace BlogAPI.Domain.Interfaces;

public interface IUserProfileRepository
{
    Task<UserProfile> CreateAsync(UserProfile profile);
    Task<UserProfile> UpdateAsync(UserProfile profile);
    Task<UserProfile> GetByIdentityUserIdAsync(string identityUserId);
    Task<UserProfile> GetByIdAsync (Guid id);
    Task DeleteAsync(Guid id);
}
