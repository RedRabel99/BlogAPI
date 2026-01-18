using BlogAPI.Application.Shared;
using BlogAPI.Domain.Abstractions;
using FluentValidation.Results;

namespace BlogAPI.Application.Extensions;

public static class ValidationResultExtensions
{
    public static Result ToValidationFailure(this ValidationResult validationResult) => 
        Result.Failure(CommonErrors.ValidationFailure,validationResult.Errors.ToErrorList());

    public static Result<T> ToValidationFailure<T>(this ValidationResult validationResult) =>
        Result<T>.Failure(CommonErrors.ValidationFailure, validationResult.Errors.ToErrorList());

    private static IReadOnlyList<SubError> ToErrorList(this List<ValidationFailure> validationFailures) =>
        validationFailures.Select(e => new SubError(Name: e.PropertyName, Description: e.ErrorMessage)).ToList();
}
