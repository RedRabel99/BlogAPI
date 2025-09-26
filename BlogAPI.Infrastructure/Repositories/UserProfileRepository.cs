using BlogAPI.Domain.Interfaces;
using BlogAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly AppDbContext _appDbContext;

    public UserProfileRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<UserProfile> CreateAsync(UserProfile profile)
    {
        _appDbContext.UserProfiles.Add(profile);
        await _appDbContext.SaveChangesAsync();
        return profile;
    }

    public async Task DeleteAsync(Guid id)
    {
        var userProfile = await _appDbContext.UserProfiles.FindAsync(id);
        if (userProfile == null) return;
        _appDbContext.UserProfiles.Remove(userProfile);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<UserProfile> GetByIdAsync(Guid id)
    {
        return await _appDbContext.UserProfiles.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<UserProfile> GetByIdentityUserIdAsync(string identityUserId)
    {
        return await _appDbContext.UserProfiles.FirstOrDefaultAsync(x => x.ApplicationUserId == identityUserId);
    }

    public async Task<UserProfile> UpdateAsync(UserProfile profile)
    {
        _appDbContext.UserProfiles.Update(profile);
        await _appDbContext.SaveChangesAsync();
        return profile;
    }
}
