using BlogAPI.Application.DTOs.Auth;
using BlogAPI.Application.Common.Validation;
using FluentValidation;

namespace BlogAPI.Application.Validators.Auth;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");
        RuleFor(x => x.NewPassword).PasswordRules();
    }
}
