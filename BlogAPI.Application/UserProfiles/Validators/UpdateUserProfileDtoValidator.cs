using BlogAPI.Application.UserProfiles.Dtos;
using BlogAPI.Application.Common.Validation;
using FluentValidation;

namespace BlogAPI.Application.UserProfiles.Validators;

public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
{
    public UpdateUserProfileDtoValidator()
    {
        RuleFor(x => x.DisplayName).CommonNameRules();
    }
}
