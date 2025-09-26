namespace BlogAPI.Domain.Interfaces;

public interface IUserInfo
{
    string Id { get; }
    string UserName { get; }
    string Email { get; }
}
