using BlogAPI.Domain.Entities;

namespace BlogAPI.Domain.Interfaces.Posts;

public interface IPostRepository
{
    Task<Post?> CreatePostAsync(Post post);
    Post? GetPost(Post post);
    Task<Post?> GetPostBySlugAndUser(Guid id, string slug);
    IQueryable<Post> GetPostQuery();
}
