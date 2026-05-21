namespace BlogAPI.Application.DTOs.Auth;

public class ConfirmEmailDto
{
    public required string Email { get; set; }
    public required string Token { get; set; }
}
