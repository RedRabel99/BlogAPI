using BlogAPI.Application.Tags.Dtos;
using BlogAPI.Application.Common.Errors;
using BlogAPI.Application.Common.Querying;
using BlogAPI.Application.Common.Validation;
using BlogAPI.Application.Interfaces;
using BlogAPI.Application.Common.Persistance;
using BlogAPI.Application.Common.Pagination;
using BlogAPI.Domain.Abstractions;
using BlogAPI.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BlogAPI.Application.Tags;

public class TagService : ITagService
{
    private readonly IAppDbContext _appDbContext;
    private readonly IPagedListFactory _pagedListFactory;
    private readonly IValidator<SearchTagQueryParametersDto> _tagQueryParametersValidator;

    public TagService(IAppDbContext appDbContext, IPagedListFactory pagedListFactory, IValidator<SearchTagQueryParametersDto> tagQueryParametersValidator)
    {
        _appDbContext = appDbContext;
        _pagedListFactory = pagedListFactory;
        _tagQueryParametersValidator = tagQueryParametersValidator;
    }

    public async Task<Result<TagDto>> GetTagByIdAsync(Guid id)
    {
        var tag = await _appDbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);

        if (tag is null)
        {
            return Result<TagDto>.Failure(TagErrors.NotFound);
        }

        return Result<TagDto>.Success(tag.ToDto());
    }

    public async Task<Result<TagDto>> GetTagByNameAsync(string name)
    {
        var tag = await _appDbContext.Tags.FirstOrDefaultAsync(t => t.TagName == name);

        if (tag is null)
        {
            return Result<TagDto>.Failure(TagErrors.NotFound);
        }

        return Result<TagDto>.Success(tag.ToDto());
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

        var query = _appDbContext.Tags
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

        var tagQuerySorting = new TagQuerySorting(queryParametersDto.SortingOrder, queryParametersDto.SortColumn);
        var tagQueryFiltering = new TagSearchQueryFilter(queryParametersDto.TagName);

        var query = _appDbContext.Tags
            .Where(t => t.Posts.Any(p => p.Id == id))
            .ApplyFiltering(tagQueryFiltering)
            .ApplySorting(tagQuerySorting)
            .Select(TagMappers.ProjectToDto);

        var result = await _pagedListFactory.CreateAsync(query, queryParametersDto.Page, queryParametersDto.PageSize);

        return Result<PagedList<TagDto>>.Success(result);
    }

    //resolve doesnt call save changes, so the one calling controlls when to save
    public async Task<List<Tag>> ResolveTagsAsync(List<string> tagNames)
    {
        var result = new List<Tag>();
        if (tagNames is null)
        {
            return result;
        }

        foreach (var tagName in tagNames)
        {
            var existingTag = await _appDbContext.Tags.FirstOrDefaultAsync(t => t.TagName == tagName);
            result.Add(
                existingTag is not null
                ? existingTag
                : new Tag { TagName = tagName }
            );
        }
        return result;
    }
}
