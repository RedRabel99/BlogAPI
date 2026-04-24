using BlogAPI.Application.DTOs.Tags;
using BlogAPI.Application.Extensions;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Mapping;
using BlogAPI.Application.Shared;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Application.Validators.Tags;
using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using BlogAPI.Domain.Interfaces.Tags;
using FluentValidation;

namespace BlogAPI.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly PagedListFactory _pagedListFactory;
    private readonly IValidator<SearchTagQueryParametersDto> _tagQueryParametersValidator;

    public TagService(ITagRepository tagRepository, PagedListFactory pagedListFactory, IValidator<SearchTagQueryParametersDto> tagQueryParametersValidator)
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

    public async Task<Result<PagedList<TagDto>>> GetTagsAsync(SearchTagQueryParametersDto tagQueryParametersDto)
    {
        var validationResult = _tagQueryParametersValidator.Validate(tagQueryParametersDto);

        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationFailure<PagedList<TagDto>>();
        }

        var tagQuerySorting = new TagQuerySorting(tagQueryParametersDto.SortingOrder, tagQueryParametersDto.SortColumn);
        var tagQueryFiltering = new TagSearchQueryFilter(tagQueryParametersDto.TagName);

        var query = _tagRepository.GetAll().Result.ApplyFiltering(tagQueryFiltering)
            .ApplySorting(tagQuerySorting)
            .Select(TagMappers.ProjectToDto);

        var result = await _pagedListFactory.CreateAsync(query, tagQueryParametersDto.Page, tagQueryParametersDto.PageSize);

        return Result<PagedList<TagDto>>.Success(result);
    }

    public Task<Result<PagedList<TagDto>>> GetTagsByPostIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
