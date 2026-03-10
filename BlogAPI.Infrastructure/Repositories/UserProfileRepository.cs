using BlogAPI.Domain.Interfaces.UserProfiles;
using BlogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using BlogAPI.Domain.Exceptions;
using BlogAPI.Infrastructure.Helper;

namespace BlogAPI.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly AppDbContext _appDbContext;

    public UserProfileRepository(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<Guid> CreateAsync(UserProfile profile)
    {
        if (profile == null)
        {
            return Guid.Empty;
        }
        try
        {
            _appDbContext.UserProfiles.Add(profile);
            await _appDbContext.SaveChangesAsync();
        }catch(DbUpdateException ex)
        {
            _appDbContext.ChangeTracker.Clear();
            if (DatabaseExceptionHelper.IsUniqueConstraintViolation(ex, "IX_UserProfile_UserName"))
            {

                throw new DuplicateUserNameException(profile.UserName);
            }

            throw;
        }catch (Exception)
        {
            _appDbContext.ChangeTracker.Clear();
            throw;
        }
        
        return profile.Id;
    }

    public async Task DeleteAsync(Guid id)
    {
        var userProfile = await _appDbContext.UserProfiles.FindAsync(id);
        if (userProfile == null) return;
        _appDbContext.UserProfiles.Remove(userProfile);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id)
    {
        return await _appDbContext.UserProfiles
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<UserProfile?> GetByApplicationUserIdAsync(string identityUserId)
    {
        return await _appDbContext.UserProfiles
            .FirstOrDefaultAsync(x => x.ApplicationUserId == identityUserId);
    }

    public IQueryable<UserProfile> GetAll()
    {
        return _appDbContext.UserProfiles;
    }

    public async Task<UserProfile?> UpdateAsync(UserProfile profile)
    {
        _appDbContext.UserProfiles.Update(profile);
        await _appDbContext.SaveChangesAsync();
        return profile;
    }
}
