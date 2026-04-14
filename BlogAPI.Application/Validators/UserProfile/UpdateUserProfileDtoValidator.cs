using BlogAPI.Application.DTOs;
using BlogAPI.Application.Validators.Shared;
using FluentValidation;

namespace BlogAPI.Application.Validators.UserProfile;

public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
{
    public UpdateUserProfileDtoValidator()
    {
        RuleFor(x => x.DisplayName).CommonNameRules();
    }
}
