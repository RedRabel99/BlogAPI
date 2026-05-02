using BlogAPI.Application.DTOs.Posts;
using FluentValidation;

namespace BlogAPI.Application.Validators.Posts;

public class UpdatePostDtoValidator : AbstractValidator<UpdatePostDto>
{
    public UpdatePostDtoValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                x.Title is not null ||
                x.Content is not null ||
                x.Excerpt is not null ||
                x.Tags is not null)
            .WithMessage("At least one field must be provided.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.")
            .When(x => x.Title is not null);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Slug cannot be empty or whitespace.")
            .MaximumLength(100)
            .WithMessage("Slug cannot exceed 100 characters.")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug can only contain lowercase letters, numbers, and hyphens.")
            .When(x => x.Slug is not null);

        RuleFor(x => x.Excerpt)
            .MaximumLength(300)
            .WithMessage("Excerpt cannot exceed 300 characters.")
            .When(x => x.Excerpt is not null);

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .WithMessage("Tag cannot be empty.")
            .MaximumLength(50)
            .WithMessage("Tag cannot exceed 50 characters.");

        RuleFor(x => x.Tags)
            .Must(tags => tags.Distinct().Count() == tags.Count)
            .WithMessage("Tags must be unique.")
            .When(x => x.Tags is not null);
    }
}
