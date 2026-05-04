using BlogAPI.Domain.Entities;

namespace BlogAPI.Domain.Interfaces.Posts;

public interface IPostRepository
{
    Task<Post?> CreatePostAsync(Post post);
    Task<Post?> GetPostAsync(Guid id);
    Task<Post?> GetPostBySlugAndUser(Guid id, string slug);
    IQueryable<Post> GetPostQuery();
    Task DeletePostAsync(Post post);
    Task DeletePostByIdAsync(Guid id);
    Task UpdatePostAsync(Post post);
}
