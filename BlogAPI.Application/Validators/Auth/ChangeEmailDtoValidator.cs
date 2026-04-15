using BlogAPI.Application.DTOs.Auth;
using FluentValidation;

namespace BlogAPI.Application.Validators.Auth;

public class ChangeEmailDtoValidator : AbstractValidator<ChangeEmailDto>
{
    public ChangeEmailDtoValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
    }
}