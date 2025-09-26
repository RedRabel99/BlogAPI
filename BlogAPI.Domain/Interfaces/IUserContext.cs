using BlogAPI.Domain.Interfaces;

namespace BlogAPI.Domain.Interfaces
{
    public interface IUserContext
    {
        bool IsAuthenticated { get; }
        string UserId { get; }
        string Email { get; }
        Task<IUserInfo> GetCurrentUserAsync();
    }
}
