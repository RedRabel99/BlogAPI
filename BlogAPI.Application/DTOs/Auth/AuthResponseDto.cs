namespace BlogAPI.Application.DTOs.Auth;

public class AuthResponseDto
{
    public required string AccessToken { get; init; }
    public int ExpiresInSeconds { get; init; }
    public required string RefreshToken { get; init; }
}
