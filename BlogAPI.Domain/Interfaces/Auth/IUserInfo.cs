namespace BlogAPI.Domain.Interfaces.Auth;

public interface IUserInfo
{
    string Id { get; }
    string UserName { get; }
    string Email { get; }
}
