using BlogAPI.Application.DTOs.Tags;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Mapping;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Tags;

namespace BlogAPI.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly PagedListFactory _pagedListFactory;

    public TagService(ITagRepository tagRepository, PagedListFactory pagedListFactory)
    {
        _tagRepository = tagRepository;
        _pagedListFactory = pagedListFactory;
    }

    public async Task<Result<TagDto>> GetTagByIdAsync(Guid id)
    {
        var tag = await _tagRepository.GetByIdAsync(id);

        if(tag is null)
        {
            return Result<TagDto>.Failure(TagErrors.TagNotFound);
        }

        var tagDto = tag.ToDto();

        return Result<TagDto>.Success(tagDto);
    }

    public async Task<Result<TagDto>> GetTagByNameAsync(string name)
    {
        var tag = await _tagRepository.GetByNameAsync(name);

        if (tag is null)
        {
            return Result<TagDto>.Failure(TagErrors.TagNotFound);
        }

        var tagDto = tag.ToDto();

        return Result<TagDto>.Success(tagDto);
    }

    public async Task<Result<TagDto>> GetTagBySlugAsync(string slug)
    {
        var tag = await _tagRepository.GetBySlugAsync(slug);

        if (tag is null)
        {
            return Result<TagDto>.Failure(TagErrors.TagNotFound);
        }

        var tagDto = tag.ToDto();

        return Result<TagDto>.Success(tagDto);
    }

    public Task<Result<PagedList<TagDto>>> GetTagsAsync(SearchTagQueryParametersDto tagQueryParametersDto)
    {
        
    }

    public Task<Result<PagedList<TagDto>>> GetTagsByPostIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
