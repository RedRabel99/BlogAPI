using BlogAPI.Domain.Entities;
namespace BlogAPI.Domain.Interfaces.UserProfiles;

public interface IUserProfileRepository
{
    Task<Guid> CreateAsync(UserProfile profile);
    Task<UserProfile?> UpdateAsync(UserProfile profile);
    Task<UserProfile?> GetByApplicationUserIdAsync(string applicationUserId);
    Task<UserProfile?> GetByIdAsync (Guid id);
    Task DeleteAsync(Guid id);
    IQueryable<UserProfile> GetAll();
}
