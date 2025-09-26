namespace BlogAPI.Domain.Models;

public class Comment : BaseEntity
{
    public string Content { get; set; }
    public Guid UserProfileId { get; set; }
    public Guid PostId {  get; set; }
    public UserProfile UserProfile { get; set; } = null!;
    public Post Post { get; set; } = null!;
}