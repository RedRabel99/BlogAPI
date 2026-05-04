using BlogAPI.Application.DTOs.Posts;
using BlogAPI.Application.Shared.Pagination;
using FluentValidation;
using Microsoft.Extensions.Options;
using BlogAPI.Application.Validators.Shared;

namespace BlogAPI.Application.Validators.Posts;

public class PostQueryParametersValidator : AbstractValidator<PostQueryParametersDto>
{
    public PostQueryParametersValidator(IOptions<PaginationOptions> paginationOptions)
    {
        var options = paginationOptions.Value;

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Usernames)
            .Must(u => u.Count <= 50).WithMessage("Cannot filter by more than 50 usernames.")
            .When(x => x.Usernames.Count > 0);

        RuleForEach(x => x.Usernames)
            .MinimumLength(3).WithMessage("Username must be at least 3 character long.")
            .MaximumLength(50).WithMessage("Username must be at most 50 characters long.")
            .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Only letters, numbers, - and _ are allowed.")
            .Must(u => !u.StartsWith('-') && !u.StartsWith('_'))
            .WithMessage("Username cannot start with - or _.");

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Tag cannot be empty.")
            .MaximumLength(50).WithMessage("Tag cannot exceed 50 characters.");

        RuleFor(x => x.SortingOrder)
            .Must(so => so.ToLower() == "asc" || so.ToLower() == "desc")
            .WithMessage("Sorting order must be one of: asc or desc");

        RuleFor(x => x.SortColumn)
            .Must(sc => sc is null || new[] { "title", "username", "createdat", "updatedat" }
                .Contains(sc.ToLower()))
            .WithMessage("SortColumn must be one of: title, username, createdAt, updatedAt.");

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
