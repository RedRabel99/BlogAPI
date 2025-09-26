using BlogAPI.Domain.Interfaces;

namespace BlogAPI.Infrastructure.Identity;

public class UserInfoAdapter : IUserInfo
{
    public UserInfoAdapter(ApplicationUser applicationUser)
    {
        Id = applicationUser.Id;
        Email = applicationUser.Email;
        UserName = applicationUser.UserName;
    }

    public string Id { get; }

    public string UserName { get; }

    public string Email { get; }
}
