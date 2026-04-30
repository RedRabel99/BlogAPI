using BlogAPI.Domain.Entities;

namespace BlogAPI.Domain.Interfaces.Tags
{
    public interface ITagRepository
    {
        Task<Tag?> GetByIdAsync(Guid id);
        Task<Tag?> GetByNameAsync(string name);
        IQueryable<Tag> GetAll();
        IQueryable<Tag> GetTagsByPostId(Guid postId);
    }
}
