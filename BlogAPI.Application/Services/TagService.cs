using BlogAPI.Application.DTOs.Tags;
using BlogAPI.Application.Extensions;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Mapping;
using BlogAPI.Application.Shared;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Tags;
using FluentValidation;

namespace BlogAPI.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly IPagedListFactory _pagedListFactory;
    private readonly IValidator<SearchTagQueryParametersDto> _tagQueryParametersValidator;

    public TagService(ITagRepository tagRepository, IPagedListFactory pagedListFactory, IValidator<SearchTagQueryParametersDto> tagQueryParametersValidator)
    {
        _tagRepository = tagRepository;
        _pagedListFactory = pagedListFactory;
        _tagQueryParametersValidator = tagQueryParametersValidator;
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

    public async Task<Result<PagedList<TagDto>>> GetTagsAsync(SearchTagQueryParametersDto queryParamsDto)
    {
        var validationResult = await _tagQueryParametersValidator.ValidateAsync(queryParamsDto);

        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure<PagedList<TagDto>>();
        }

        var tagQuerySorting = new TagQuerySorting(queryParamsDto.SortingOrder, queryParamsDto.SortColumn);
        var tagQueryFiltering = new TagSearchQueryFilter(queryParamsDto.TagName);

        var query = _tagRepository.GetAll()
            .ApplyFiltering(tagQueryFiltering)
            .ApplySorting(tagQuerySorting)
            .Select(TagMappers.ProjectToDto);

        var result = await _pagedListFactory.CreateAsync(query, queryParamsDto.Page, queryParamsDto.PageSize);

        return Result<PagedList<TagDto>>.Success(result);
    }

    public async Task<Result<PagedList<TagDto>>> GetTagsByPostIdAsync(Guid id, SearchTagQueryParametersDto queryParametersDto)
    {
        var validationResult = await _tagQueryParametersValidator.ValidateAsync(queryParametersDto);

        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure<PagedList<TagDto>>();
        }

        var tagQuerySorting = new TagQuerySorting(queryParametersDto.SortingOrder,
            queryParametersDto.SortColumn);
        var tagQueryFiltering = new TagSearchQueryFilter(queryParametersDto.TagName);

        var query = _tagRepository.GetTagsByPostId(id)
            .ApplyFiltering(tagQueryFiltering)
            .ApplySorting(tagQuerySorting)
            .Select(TagMappers.ProjectToDto);

        var result = await _pagedListFactory.CreateAsync(query, queryParametersDto.Page, queryParametersDto.PageSize);

        return Result<PagedList<TagDto>>.Success(result);
    }

    public async Task<List<Tag>> ResolveTagsAsync(List<string> tagNames)
    {
        var result = new List<Tag>();
        if(tagNames is null)
        {
            return result;
        }

        foreach( var tagName in tagNames)
        {
            var existingTag = await _tagRepository.GetByNameAsync(tagName);
            result.Add(
                existingTag is not null 
                ? existingTag 
                : new Tag { TagName = tagName }
            );
        }
        return result;
    }
}
