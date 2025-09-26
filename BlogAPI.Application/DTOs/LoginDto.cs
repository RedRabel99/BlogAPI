namespace BlogAPI.Application.DTOs;

public record LoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}
