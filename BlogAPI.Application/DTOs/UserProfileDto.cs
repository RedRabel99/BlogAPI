namespace BlogAPI.Application.DTOs;

public record UserProfileDto
{
    public Guid Id { get; set; }
    public string IdentityUserId { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }
}
