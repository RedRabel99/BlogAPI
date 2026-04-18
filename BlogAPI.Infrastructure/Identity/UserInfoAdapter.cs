using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Auth;

namespace BlogAPI.Infrastructure.Identity;

public class UserInfoAdapter : IUserInfo
{
    public UserInfoAdapter(ApplicationUser applicationUser)
    {
        Id = applicationUser.Id;
        Email = applicationUser.Email;
        UserName = applicationUser.UserName;
        UserProfile = applicationUser.UserProfile;
    }

    public string Id { get; }

    public string UserName { get; }

    public string Email { get; }
    
    public UserProfile UserProfile { get; }
}
