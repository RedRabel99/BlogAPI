using BlogAPI.Application.Shared.Pagination;
using FluentValidation;

namespace BlogAPI.Application.Validators.Shared;

public static class PaginationValidator
{
    public static IRuleBuilder<T, int?> ApplyPageSizeRules<T>(
        this IRuleBuilder<T, int?> ruleBuilder, PaginationOptions options)
    {
       return ruleBuilder.GreaterThanOrEqualTo(options.MinPageSize)
                .WithMessage($"Page size can not be lesser than {options.MinPageSize}")
            .LessThanOrEqualTo(options.MaxPageSize)
                .WithMessage($"Page size can not be greater than {options.MaxPageSize}")
            .When(x => x is not null);
    }

    public static IRuleBuilder<T, int?> ApplyPageRules<T>(
        this IRuleBuilder<T, int?> ruleBuilder)
    {
        return ruleBuilder.GreaterThanOrEqualTo(1)
                .WithMessage($"Page number can not be lesser than 1")
            .When(x => x is not null);
    }
}
