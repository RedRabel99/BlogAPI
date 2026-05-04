using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Posts;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    public readonly AppDbContext _context;

    public PostRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Post?> CreatePostAsync(Post post)
    {
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task DeletePostAsync(Post post)
    {
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePostByIdAsync(Guid id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post is null)
        {
            return;
        }
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
    }

    public async Task<Post?> GetPostAsync(Guid id)
    {
        var post = await _context.Posts
            .Include(x => x.Tags)
            .Include(x => x.UserProfile)
            .FirstOrDefaultAsync(x => x.Id == id);
        return post;
    }

    public async Task<Post?> GetPostBySlugAndUser(Guid id, string slug)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(x => x.UserProfileId == id && x.Slug.Equals(slug));
        return post;
    }

    public IQueryable<Post> GetPostQuery()
    {
        return _context.Posts;
    }

    public async Task UpdatePostAsync(Post post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
    }
}
