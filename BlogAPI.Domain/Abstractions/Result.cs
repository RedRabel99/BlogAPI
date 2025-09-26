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
    }

    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}

public class Result<T> : Result
{
    private Result(bool isSuccess, T value, Error error) : base(isSuccess, error)
    {
        Value = value;
    }
    public T Value { get;}

    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static new Result<T> Failure(Error error) => new(false, default!, error);
}
