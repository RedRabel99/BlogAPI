namespace BlogAPI.Application.DTOs.Auth;

public class RefreshRequestDto
{
    public required string RefreshToken { get; init; }
}
