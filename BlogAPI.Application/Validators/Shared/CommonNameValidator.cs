using FluentValidation;

namespace BlogAPI.Application.Validators.Shared;

public static class CommonNameValidator
{
    public static IRuleBuilder<T, string> CommonNameRules<T>(this IRuleBuilder<T, string> ruleBuilder) {
        {
            return ruleBuilder
            .NotEmpty()
            .MinimumLength(3).WithMessage("Must be atleast 3 characters long")
            .MaximumLength(40).WithMessage("Must be less than 41");
        }
    }
}