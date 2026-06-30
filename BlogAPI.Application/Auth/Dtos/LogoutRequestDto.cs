namespace BlogAPI.Application.Auth.Dtos;

public class LogoutRequestDto
{
    public required string RefreshToken { get; init; }
}
