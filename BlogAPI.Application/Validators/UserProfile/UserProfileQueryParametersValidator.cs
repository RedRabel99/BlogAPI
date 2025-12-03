using BlogAPI.Application.DTOs;
using FluentValidation;

namespace BlogAPI.Application.Validators.UserProfile;

public class UserProfileQueryParametersValidator :
    AbstractValidator<UserProfileQueryParametersDto>
{
    public UserProfileQueryParametersValidator()
    {
        RuleFor(x => x.UserName)
            .MinimumLength(1)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.UserName));
        RuleFor(x => x.DisplayName)
            .MinimumLength(1)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.DisplayName));
    }
}
