namespace BlogAPI.Application.DTOs;

public record RegisterDto
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
}