using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Errors
{
    public static class CommonErrors
    {
        public static Error ValidationFailure =
            Error.Validation("Validation failure", "One or more validation errors occured");
    }
}
