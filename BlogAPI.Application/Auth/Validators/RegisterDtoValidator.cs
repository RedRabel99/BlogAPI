using BlogAPI.Application.Auth.Dtos;
using BlogAPI.Application.Common.Validation;
using FluentValidation;

namespace BlogAPI.Application.Auth.Validators; 

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Username).UsernameRules();
        RuleFor(x => x.DisplayName).CommonNameRules();
        RuleFor(x => x.Password).PasswordRules();
    }
}
