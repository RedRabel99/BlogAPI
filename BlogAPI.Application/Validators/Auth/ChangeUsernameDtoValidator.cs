using BlogAPI.Application.DTOs;
using BlogAPI.Application.Validators.Shared;
using FluentValidation;

namespace BlogAPI.Application.Validators.Auth;

public class ChangeUsernameDtoValidator : AbstractValidator<ChangeUsernameDto>
{
    public ChangeUsernameDtoValidator()
    {
        RuleFor(x => x.Username).UsernameRules();
    }
}
