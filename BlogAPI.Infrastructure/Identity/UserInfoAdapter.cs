using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Auth;

namespace BlogAPI.Infrastructure.Identity;

public class UserInfoAdapter : IUserInfo
{
    public UserInfoAdapter(ApplicationUser applicationUser, string userProfileId)
    {
        Id = applicationUser.Id;
        Email = applicationUser.Email;
        UserName = applicationUser.UserName;
        UserProfileId = userProfileId;
    }

    public string Id { get; }

    public string UserName { get; }

    public string Email { get; }

    public string UserProfileId { get; }
}
