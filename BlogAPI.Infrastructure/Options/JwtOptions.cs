using System.ComponentModel.DataAnnotations;

namespace BlogAPI.Infrastructure.Options;

public sealed class JwtOptions
{
    [Required]
    [Range(1, int.MaxValue)]
    public int AccessTokenMinutes { get; init; }
    [Required]
    [Range(1, int.MaxValue)]
    public int RefreshTokenDays { get; init; }

    [Required]
    public required string Secret { get; init; }
    [Required]
    public required string Issuer { get; init; }
    [Required]
    public required string Audience { get; init; }
}
