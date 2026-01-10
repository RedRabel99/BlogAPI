using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Web.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException();
        }
        if (result.Error.Type == ErrorType.Validation && result.SubErrors?.Any() is true)
        {
            return GetValidationProblemDetailsAsResult(result);
        }

        return GetProblemDetailsAsResult(result);
    }

    private static IDictionary<string, string[]> ToValidationErrors(this IReadOnlyList<SubError> subErrors) =>
        subErrors.Where(e => !String.IsNullOrEmpty(e.Name)).GroupBy(e => e.Name!).ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

    private static string? GetType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => "https://tools.ietf.org/doc/html/rfc7231#section-6.5.1",
            ErrorType.NotFound => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            ErrorType.Conflict => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8",
            ErrorType.Forbidden => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
            _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"

        };

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError

        };

    private static string GetTitle(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => "Bad Request",
            ErrorType.NotFound => "Not Found",
            ErrorType.Conflict => "Conflict",
            ErrorType.Forbidden => "Forbidden",
            _ => "Server Error"
        };

    private static IResult GetValidationProblemDetailsAsResult(Result result)
    {
        return Results.ValidationProblem(
                statusCode: StatusCodes.Status400BadRequest,
                errors: result.SubErrors.ToValidationErrors(),
                title: "Bad request",
                type: "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"
                );
    }

    private static IResult GetProblemDetailsAsResult(Result result)
    {
        return Results.Problem(
            statusCode: GetStatusCode(result.Error.Type),
            title: GetTitle(result.Error.Type),
            type: GetType(result.Error.Type),
            extensions: new Dictionary<string, object?>
            {
                {"errors", new[]{result.Error}}
            }
            );
    }
}
