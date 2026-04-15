using BlogAPI.Application.DTOs.UserProfiles;
using BlogAPI.Application.Shared.Pagination;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace BlogAPI.Application.Validators.UserProfile;

public class UserProfileQueryParametersValidator :
    AbstractValidator<UserProfileQueryParametersDto>
{
    public UserProfileQueryParametersValidator(IOptions<PaginationOptions> paginationOptions)
    {
        var options = paginationOptions.Value;
        RuleFor(x => x.UserName)
            .MinimumLength(1)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.UserName))
            .WithMessage("Username length must be within 1 to 50 characters");
        RuleFor(x => x.DisplayName)
            .MinimumLength(1)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.DisplayName))
            .WithMessage("Display name lenght must be within 1 to 50 characters");
        RuleFor(x => x.SortingOrder)
            .Must(so => so.ToLower() == "asc" || so.ToLower() == "desc")
            .WithMessage("Sorting order must be one of: asc or desc");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(options.MinPageSize)
                .WithMessage($"Page size can not be lesser than {options.MinPageSize}")
            .LessThanOrEqualTo(options.MaxPageSize)
                .WithMessage($"Page size can not be greater than {options.MaxPageSize}")
            .When(x => x is not null); 
        
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
                .WithMessage($"Page number can not be lesser than 1")
            .When(x=> x is not null);
    }
}
