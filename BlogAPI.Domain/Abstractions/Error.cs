namespace BlogAPI.Domain.Abstractions;

public sealed class Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    Error(string code, string description, ErrorType type, string? propertyName = null)
    {
        Code = code;
        Description = description;
        Type = type;
        PropertyName = propertyName;
    }

    public string Code { get; }
    public string Description { get; }
    public string? PropertyName { get; }
    public ErrorType Type { get; }

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);
    public static Error Validation(string code, string description, string? propertyName = null) =>
        new(code, description, ErrorType.Validation, propertyName);
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
}