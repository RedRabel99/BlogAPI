using BlogAPI.Application.DTOs;
using BlogAPI.Application.Validators.Shared;
using FluentValidation;

namespace BlogAPI.Application.Validators.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.UserName).UsernameRules();
        RuleFor(x => x.DisplayName).CommonNameRules();
        RuleFor(x => x.Password).PasswordRules();
    }
}
