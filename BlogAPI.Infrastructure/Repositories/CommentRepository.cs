using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Comments;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Comment> CreateAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment?> GetByIdAsync(Guid id)
    {
        return await _context.Comments
            .Include(c => c.UserProfile)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task UpdateAsync(Comment comment)
    {
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Comment comment)
    {
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment is null)
        {
            return;
        }
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }

    public IQueryable<Comment> GetQuery()
    {
        return _context.Comments;
    }
}
