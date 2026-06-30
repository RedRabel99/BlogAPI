using BlogAPI.Application.Auth.Dtos;
using BlogAPI.Application.Common.Validation;
using FluentValidation;

namespace BlogAPI.Application.Auth.Validators;

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.OldPassword).NotEmpty();
        RuleFor(x => x.NewPassword).PasswordRules();
    }
}