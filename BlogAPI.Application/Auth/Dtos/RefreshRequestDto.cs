namespace BlogAPI.Application.Auth.Dtos;

public class RefreshRequestDto
{
    public required string RefreshToken { get; init; }
}
