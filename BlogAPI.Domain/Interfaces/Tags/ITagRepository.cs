using BlogAPI.Domain.Entities;

namespace BlogAPI.Domain.Interfaces.Tags
{
    public interface ITagRepository
    {
        Task<Tag> GetByIdAsync(Guid id);
        Task<Tag> GetByNameAsync(string name);
        Task<Tag> GetBySlugAsync(string slug);
        Task<IQueryable<Tag>> GetAll();
        Task<IQueryable<Tag>> GetTagsByPostIdAsync(Guid postId);
    }
}
