using BlogAPI.Application.DTOs.Tags;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Application.Validators.Shared;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace BlogAPI.Application.Validators.Tags;

public class TagQueryParametersValidator : AbstractValidator<SearchTagQueryParametersDto>
{
    public TagQueryParametersValidator(IOptions<PaginationOptions> options)
    {
        RuleFor(x => x.TagName)
            .MinimumLength(1)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.TagName))
            .WithMessage("Tag name length must be within 1 to 50 characters");

        RuleFor(x => x.PageSize).ApplyPageSizeRules(options.Value);
        RuleFor(x => x.Page).ApplyPageRules();
    }
}
