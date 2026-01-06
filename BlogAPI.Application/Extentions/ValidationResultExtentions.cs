using BlogAPI.Domain.Abstractions;
using FluentValidation.Results;

namespace BlogAPI.Application.Extentions;

public static class ValidationResultExtentions
{
    public static Result ToValidationFailure(this ValidationResult validationResult) => 
        Result.ValidationFailure("Validation.Failure", validationResult.Errors.ToErrorList());

    public static Result<T> ToValidationFailure<T>(this ValidationResult validationResult) =>
        Result<T>.ValidationFailure("Validation.Failure", validationResult.Errors.ToErrorList());

    private static IReadOnlyList<Error> ToErrorList(this List<ValidationFailure> validationFailures) =>
        validationFailures.Select(e => Error.Validation(e.ErrorCode, e.ErrorMessage, e.PropertyName)).ToList();
}
