using BlogAPI.Application.DTOs;
using FluentValidation;

namespace BlogAPI.Application.Validators.Auth;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage(x => $"{x.Email} is not a valid email");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password can not be empty");
    }
}
