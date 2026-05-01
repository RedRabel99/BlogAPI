using BlogAPI.Application.DTOs.UserProfiles;
using BlogAPI.Application.Shared.Pagination;
using FluentValidation;
using Microsoft.Extensions.Options;
using BlogAPI.Application.Validators.Shared;

namespace BlogAPI.Application.Validators.UserProfile;

public class UserProfileQueryParametersValidator :
    AbstractValidator<UserProfileQueryParametersDto>
{
    public UserProfileQueryParametersValidator(IOptions<PaginationOptions> paginationOptions)
    {
        var options = paginationOptions.Value;
        RuleFor(x => x.UserName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.UserName))
            .WithMessage("Username length must be within 1 to 50 characters");
        RuleFor(x => x.DisplayName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.DisplayName))
            .WithMessage("Display name lenght must be within 1 to 50 characters");
        RuleFor(x => x.SortingOrder)
            .Must(so => so.ToLower() == "asc" || so.ToLower() == "desc")
            .WithMessage("Sorting order must be one of: asc or desc");

        RuleFor(x => x.PageSize).ApplyPageSizeRules(options);
        RuleFor(x => x.Page).ApplyPageRules();
    }
}
