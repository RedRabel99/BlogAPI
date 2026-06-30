using BlogAPI.Application.Tags.Dtos;
using BlogAPI.Application.Common.Pagination;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;

namespace BlogAPI.Application.Tags;

public interface ITagService
{
    Task<Result<PagedList<TagDto>>> GetTagsAsync(SearchTagQueryParametersDto queryParamsDto);
    Task<Result<TagDto>> GetTagByIdAsync(Guid id);
    Task<Result<TagDto>> GetTagByNameAsync(string name);
    Task<Result<PagedList<TagDto>>> GetTagsByPostIdAsync(Guid id, SearchTagQueryParametersDto queryParametersDto);
    Task<List<Tag>> ResolveTagsAsync(List<string> tagNames);
}
