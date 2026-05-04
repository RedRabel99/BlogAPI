using BlogAPI.Application.DTOs.Posts;
using FluentValidation;

namespace BlogAPI.Application.Validators.Posts;

public class CreatePostDtoValidator : AbstractValidator<CreatePostDto>
{
    public CreatePostDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotNull()
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");

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
            .WithMessage("Tags must be unique.");
    }
}
