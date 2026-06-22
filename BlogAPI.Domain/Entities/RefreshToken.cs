namespace BlogAPI.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string ApplicationUserId { get; set; }
    public string TokenHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; } // for future reuse detection
    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}
