namespace BlogAPI.Domain.Models;

public class UserProfile : BaseEntity
{
    public string ApplicationUserId { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
