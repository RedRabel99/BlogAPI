namespace BlogAPI.Application.DTOs.Auth;

public record AccessTokenResultDto(string AccessToken, int ExpiresInSeconds);
