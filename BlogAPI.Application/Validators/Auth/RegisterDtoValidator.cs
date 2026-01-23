using BlogAPI.Application.DTOs;
using BlogAPI.Application.Validators.Shared;
using FluentValidation;

namespace BlogAPI.Application.Validators.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.UserName)
            .CommonNameRules()
            .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Only letters, numbers, -, _ are allowed")
            .Must(x => !x.StartsWith('-') || !x.StartsWith('_')).WithMessage("Cannot start with - or _ characters");

        RuleFor(x => x.DisplayName).CommonNameRules();
        RuleFor(x => x.Password)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}
