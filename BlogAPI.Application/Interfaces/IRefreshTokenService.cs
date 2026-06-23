using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Interfaces.Auth;

public record RefreshRotationResult(string NewRefreshToken, string ApplicationUserId);

public interface IRefreshTokenService
{
    Task<string> IssueAsync(string applicationUserId, CancellationToken ct);
    Task<Result<RefreshRotationResult>> RotateAsync(string refreshToken, CancellationToken ct);
    Task RevokeAsync(string refreshToken, CancellationToken ct);
}
