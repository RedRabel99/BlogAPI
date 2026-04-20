using BlogAPI.Application.DTOs.Auth;
using BlogAPI.Domain.Entities;
using BlogAPI.Infrastructure;
using BlogAPI.Infrastructure.Identity;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.IntegrationTests.TestData;

public class TestDataSeeder
{
    private readonly AppDbContext _appDbContext;
    private readonly IUserManager _userManager;
    private readonly Faker _faker;

    private readonly List<UserProfile> _userProfiles = new();
    private readonly List<ApplicationUser> _applicationUsers = new();

    public TestDataSeeder(AppDbContext appDbContext, IUserManager userManager)
    {
        _appDbContext = appDbContext;
        _userManager = userManager;
        _faker = new Faker();
    }

    public async Task SeedAsync()
    {
        await SeedUsers();
        await _appDbContext.SaveChangesAsync();
    }

    private async Task SeedUsers(int count = 10, string password = "Password123!")
    {
        var userFaker = new Faker<RegisterDto>()
            .CustomInstantiator(f => new RegisterDto
            {
                Username = f.Internet.UserName(),
                Email = f.Internet.Email(),
                DisplayName = f.Internet.UserName(),
                Password = password
            });
        
        var registerDtos = userFaker.Generate(count);

        foreach(var dto in registerDtos)
        {
            await _userManager.CreateUserAsync(dto);
        }

        _applicationUsers.AddRange(_appDbContext.Users.Include(u => u.UserProfile).ToList());
        _userProfiles.AddRange(_appDbContext.UserProfiles.ToList());
    }
    // Helper methods to get seeded entities
    public UserProfile GetUserProfile(int index = 0) => _userProfiles[index];
    public ApplicationUser GetApplicationUser(int index = 0) => _applicationUsers[index];

    public int GetUserProfilesLength() => _userProfiles.Count;
    public int GetApplicationUsersLength() => _applicationUsers.Count;
}
