namespace BlogAPI.Application.DTOs.UserProfile;

public record UserProfileDto
{
    public Guid Id { get; set; }
    public string ApplicationUserId { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }
}
