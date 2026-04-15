using BlogAPI.Application.DTOs.Auth;
using BlogAPI.Application.Validators.Shared;
using FluentValidation;

namespace BlogAPI.Application.Validators.Auth;

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.OldPassword).NotEmpty();
        RuleFor(x => x.NewPassword).PasswordRules();
    }
}