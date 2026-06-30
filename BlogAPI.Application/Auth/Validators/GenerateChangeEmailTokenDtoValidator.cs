using BlogAPI.Application.Auth.Dtos;
using FluentValidation;

namespace BlogAPI.Application.Auth.Validators;

public class GenerateChangeEmailTokenDtoValidator : AbstractValidator<GenerateChangeEmailTokenDto>
{
    public GenerateChangeEmailTokenDtoValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
    }
}