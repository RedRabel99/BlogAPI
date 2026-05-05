using BlogAPI.Domain.Entities;

namespace BlogAPI.Domain.Interfaces.Comments;

public interface ICommentRepository
{
    Task<Comment> CreateAsync(Comment comment);
    Task<Comment?> GetByIdAsync(Guid id);
    Task UpdateAsync(Comment comment);
    Task DeleteByIdAsync(Guid id);
    Task DeleteAsync(Comment comment);
    IQueryable<Comment> GetQuery();
}
