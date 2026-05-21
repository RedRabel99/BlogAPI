using BlogAPI.Application.DTOs.Auth;
using FluentValidation;

namespace BlogAPI.Application.Validators.Auth;

public class ConfirmEmailDtoValidator : AbstractValidator<ConfirmEmailDto>
{
    public ConfirmEmailDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");
    }
}
