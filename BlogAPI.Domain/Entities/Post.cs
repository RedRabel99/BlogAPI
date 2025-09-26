namespace BlogAPI.Domain.Models;

public class Post : BaseEntity
{
    public string Title { get; set; }
    public string Body { get; set; }
    public Guid UserProfileId { get; set; }
    public UserProfile UserProfile {  get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}