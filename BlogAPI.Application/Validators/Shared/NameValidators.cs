using FluentValidation;

namespace BlogAPI.Application.Validators.Shared;

public static class NameValidators
{
    public static IRuleBuilder<T, string> CommonNameRules<T>(this IRuleBuilder<T, string> ruleBuilder) {
            return ruleBuilder
            .NotEmpty()
            .MinimumLength(3).WithMessage("Must be atleast 3 characters long")
            .MaximumLength(40).WithMessage("Must be less than 41");
    }

    public static IRuleBuilder<T, string> UsernameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .CommonNameRules()
            .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Only letters, numbers, -, _ are allowed")
            .Must(x => !x.StartsWith('-') || !x.StartsWith('_')).WithMessage("Cannot start with - or _ characters");
    }
}