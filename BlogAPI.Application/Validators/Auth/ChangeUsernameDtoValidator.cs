using BlogAPI.Application.DTOs.Auth;
using BlogAPI.Application.Common.Validation;
using FluentValidation;

namespace BlogAPI.Application.Validators.Auth;

public class ChangeUsernameDtoValidator : AbstractValidator<ChangeUsernameDto>
{
    public ChangeUsernameDtoValidator()
    {
        RuleFor(x => x.Username).UsernameRules();
    }
}
