using BlogAPI.Application.DTOs.Comments;
using FluentValidation;

namespace BlogAPI.Application.Validators.Comments;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.Content)
            .NotNull()
            .NotEmpty().WithMessage("Content is required.")
            .Must(c => !string.IsNullOrWhiteSpace(c)).WithMessage("Content cannot be whitespace only.")
            .MaximumLength(2000).WithMessage("Content cannot exceed 2000 characters.");
    }
}
