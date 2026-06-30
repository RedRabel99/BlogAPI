using BlogAPI.Application.Auth.Dtos;
using FluentValidation;

namespace BlogAPI.Application.Auth.Validators;

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
