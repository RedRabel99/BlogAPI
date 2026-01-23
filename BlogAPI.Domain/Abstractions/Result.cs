namespace BlogAPI.Domain.Abstractions;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if ((isSuccess && error != Error.None) ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }
        IsSuccess = isSuccess;
        Error = error;
        SubErrors = new List<SubError>();
    }

    protected Result(bool isSuccess, Error error, IReadOnlyList<SubError> subErrors)
    {
        if ((isSuccess && error != Error.None) ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }
        IsSuccess = isSuccess;
        Error = error;
        SubErrors = subErrors ?? new List<SubError>();
    }

    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;

    public Error Error { get; }
    public IReadOnlyList<SubError> SubErrors { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(Error error, IReadOnlyList<SubError> subErrors)
        => new(false, error, subErrors);
}

public class Result<T> : Result
{
    private Result(bool isSuccess, T value, Error error) : base(isSuccess, error)
    {
        Value = value;
    }

    private Result(bool isSuccess,T value, Error error, IReadOnlyList<SubError> subErrors)
        : base(isSuccess, error, subErrors)
    {
        Value = value;
    }

    public T Value { get;}

    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static new Result<T> Failure(Error error) => new(false, default!, error);
    public static new Result<T> Failure(Error error, IReadOnlyList<SubError> subErrors)
        => new(false, default!, error, subErrors);
}

public record SubError(string  Name, string Description);

public static class ValidationErrors
{
    public static readonly Error ValidationError = Error.Validation("Validation.Failure", "One or more validation errors occured");
}