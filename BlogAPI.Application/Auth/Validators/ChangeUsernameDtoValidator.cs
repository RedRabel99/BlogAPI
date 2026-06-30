using BlogAPI.Application.Auth.Dtos;
using BlogAPI.Application.Common.Validation;
using FluentValidation;

namespace BlogAPI.Application.Auth.Validators;

public class ChangeUsernameDtoValidator : AbstractValidator<ChangeUsernameDto>
{
    public ChangeUsernameDtoValidator()
    {
        RuleFor(x => x.Username).UsernameRules();
    }
}
