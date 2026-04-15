namespace BlogAPI.Application.DTOs.Auth;

public record RegisterDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
}