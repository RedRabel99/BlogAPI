namespace BlogAPI.Application.UserProfiles.Dtos;

public record UserProfileDto
{
    public Guid Id { get; set; }
    public string ApplicationUserId { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }
}
