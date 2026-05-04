namespace BlogAPI.Application.Shared.UserProfile;

public class UserProfileQueryFiltering(string? username, string? displayName)
    : IQueryFilter<Domain.Entities.UserProfile>
{
    public string? Username { get; set; } = username;
    public string? Displayname { get; set; } = displayName;

    public IQueryable<Domain.Entities.UserProfile> Apply(IQueryable<Domain.Entities.UserProfile> query)
    {
        if (!string.IsNullOrWhiteSpace(Username))
        {
            query = query.Where(u => u.Username.Contains(Username));
        }

        if (!string.IsNullOrWhiteSpace(Displayname))
        {
            query = query.Where(u => u.DisplayName.Contains(Displayname));
        }

        return query;
    }
}
