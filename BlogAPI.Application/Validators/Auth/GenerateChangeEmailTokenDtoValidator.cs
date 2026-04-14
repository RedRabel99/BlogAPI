using BlogAPI.Application.DTOs;
using FluentValidation;

namespace BlogAPI.Application.Validators.Auth;

public class GenerateChangeEmailTokenDtoValidator : AbstractValidator<GenerateChangeEmailTokenDto>
{
    public GenerateChangeEmailTokenDtoValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
    }
}