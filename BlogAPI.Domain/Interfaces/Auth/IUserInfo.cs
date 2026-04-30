using BlogAPI.Domain.Entities;

namespace BlogAPI.Domain.Interfaces.Auth;

public interface IUserInfo
{
    string Id { get; }
    string UserName { get; }
    string UserProfileId { get; }
    string Email { get; }
}
