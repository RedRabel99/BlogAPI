namespace BlogAPI.Application.Auth.Dtos;

public class ConfirmEmailDto
{
    public required string Email { get; set; }
    public required string Token { get; set; }
}
