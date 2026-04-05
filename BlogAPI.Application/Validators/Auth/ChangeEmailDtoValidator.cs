using BlogAPI.Application.DTOs;
using FluentValidation;

namespace BlogAPI.Application.Validators.Auth;

public class ChangeEmailDtoValidator : AbstractValidator<GenerateChangeEmailTokenDto>
{
    public ChangeEmailDtoValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
    }
}