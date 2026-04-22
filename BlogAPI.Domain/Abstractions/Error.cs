namespace BlogAPI.Domain.Abstractions;

public sealed class Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);
    public static Error Unauthorized(string code, string description) =>
        new(code, description, ErrorType.Unauthorized);
    public static Error Validation(string code, string description, string? propertyName = null) =>
        new(code, description, ErrorType.Validation);
    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
    public static Error Internal(string code, string description) =>
        new(code, description, ErrorType.Internal);
    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);
}


public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Internal = 4,
    Forbidden = 5,
    Unauthorized = 6
}