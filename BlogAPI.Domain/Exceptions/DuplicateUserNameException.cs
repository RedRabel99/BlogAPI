namespace BlogAPI.Domain.Exceptions;

public class DuplicateUserNameException : Exception
{
    public DuplicateUserNameException(string userName) 
        : base($"User name '{userName}' already exists.") { }
}
