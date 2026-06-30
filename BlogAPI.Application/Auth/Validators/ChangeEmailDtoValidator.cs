using BlogAPI.Application.Auth.Dtos;
using FluentValidation;

namespace BlogAPI.Application.Auth.Validators;

public class ChangeEmailDtoValidator : AbstractValidator<ChangeEmailDto>
{
    public ChangeEmailDtoValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
    }
}