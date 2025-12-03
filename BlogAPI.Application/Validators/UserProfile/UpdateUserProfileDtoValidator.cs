using BlogAPI.Application.DTOs;
using BlogAPI.Application.Validators.Shared;
using FluentValidation;

namespace BlogAPI.Application.Validators.UserProfile;

public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
{
    public UpdateUserProfileDtoValidator()
    {
        RuleFor(x => x.UserName)
            .CommonNameRules()
            .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Only letters, numbers, -, _ are allowed")
            .Must(x => !x.StartsWith('-') || !x.StartsWith('_')).WithMessage("Cannot start with - or _ characters");

        RuleFor(x => x.DisplayName).CommonNameRules();
    }
}
