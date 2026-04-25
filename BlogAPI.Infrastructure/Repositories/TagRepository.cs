using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Tags;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Infrastructure.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly AppDbContext _appDbContext;

        public TagRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Tag?> GetByIdAsync(Guid id)
        {
            var tag = await _appDbContext.Tags.FirstOrDefaultAsync(x => x.Id == id);
            return tag;
        }

        public async Task<Tag?> GetByNameAsync(string name)
        {
            var tag = await _appDbContext.Tags.FirstOrDefaultAsync(x => x.TagName == name);
            return tag;
        }

        public async Task<Tag?> GetBySlugAsync(string slug)
        {
            var tag = await _appDbContext.Tags.FirstOrDefaultAsync(x => x.Slug == slug);
            return tag;
        }

        public  IQueryable<Tag> GetAll()
        {
            return _appDbContext.Tags;
        }

        public IQueryable<Tag> GetTagsByPostId(Guid postId)
        {
            return _appDbContext.Tags.Where(x => x.Posts.Any(p => p.Id == postId));
        }
    }
}
