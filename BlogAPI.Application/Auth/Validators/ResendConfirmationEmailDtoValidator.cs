using BlogAPI.Application.Auth.Dtos;
using FluentValidation;

namespace BlogAPI.Application.Auth.Validators;

public class ResendConfirmationEmailDtoValidator : AbstractValidator<ResendConfirmationEmailDto>
{
    public ResendConfirmationEmailDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
