namespace BlogAPI.Domain.Entities;

public class Tag : BaseEntity
{
    public string TagName { get; set; }
    public string Slug { get; set; }
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}