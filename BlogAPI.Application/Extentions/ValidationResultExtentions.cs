using BlogAPI.Domain;
using BlogAPI.Domain.Abstractions;
using FluentValidation.Results;

namespace BlogAPI.Application.Extentions;

public static class ValidationResultExtentions
{
    public static Result ToValidationFailure(this ValidationResult validationResult) => 
        Result.Failure(ValidationErrors.ValidationError,validationResult.Errors.ToErrorList());

    public static Result<T> ToValidationFailure<T>(this ValidationResult validationResult) =>
        Result<T>.Failure(ValidationErrors.ValidationError, validationResult.Errors.ToErrorList());

    private static IReadOnlyList<SubError> ToErrorList(this List<ValidationFailure> validationFailures) =>
        validationFailures.Select(e => new SubError(Name: e.PropertyName, Description: e.ErrorMessage)).ToList();
}
