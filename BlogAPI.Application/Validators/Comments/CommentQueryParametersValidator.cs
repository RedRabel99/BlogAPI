using BlogAPI.Application.DTOs.Comments;
using BlogAPI.Application.Shared.Pagination;
using BlogAPI.Application.Validators.Shared;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace BlogAPI.Application.Validators.Comments;

public class CommentQueryParametersValidator : AbstractValidator<CommentQueryParametersDto>
{
    public CommentQueryParametersValidator(IOptions<PaginationOptions> paginationOptions)
    {
        var options = paginationOptions.Value;

        RuleFor(x => x.Author)
            .MinimumLength(3).WithMessage("Author username must be at least 3 characters long.")
            .MaximumLength(50).WithMessage("Author username must be at most 50 characters long.")
            .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Only letters, numbers, - and _ are allowed.")
            .Must(u => !u!.StartsWith('-') && !u.StartsWith('_'))
            .WithMessage("Author username cannot start with - or _.")
            .When(x => !string.IsNullOrEmpty(x.Author));

        RuleFor(x => x.SortingOrder)
            .Must(so => so.ToLower() == "asc" || so.ToLower() == "desc")
            .WithMessage("Sorting order must be one of: asc or desc.");

        RuleFor(x => x.SortColumn)
            .Must(sc => sc is null || new[] { "createdat", "updatedat" }.Contains(sc.ToLower()))
            .WithMessage("SortColumn must be one of: createdAt, updatedAt.");

        RuleFor(x => x.PageSize).ApplyPageSizeRules(options);
        RuleFor(x => x.Page).ApplyPageRules();

        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("From date cannot be later than To date.");

        RuleFor(x => x.From)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.From.HasValue)
            .WithMessage("From date cannot be in the future.");

        RuleFor(x => x.To)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.To.HasValue)
            .WithMessage("To date cannot be in the future.");
    }
}
