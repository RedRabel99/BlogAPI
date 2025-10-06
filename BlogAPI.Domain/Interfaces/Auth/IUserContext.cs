namespace BlogAPI.Domain.Interfaces.Auth
{
    public interface IUserContext
    {
        bool IsAuthenticated { get; }
        string UserId { get; }
        string Email { get; }
        Task<IUserInfo> GetCurrentUserAsync();
    }
}
