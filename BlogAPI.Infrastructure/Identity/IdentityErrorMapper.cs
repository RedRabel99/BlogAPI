using BlogAPI.Domain.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace BlogAPI.Infrastructure.Identity;

public static class IdentityErrorMapper
{
    public static Error Map(this IdentityError identityError)
    {
        if (identityError.Code.StartsWith("Duplicate"))
        {
            return Error.Conflict(identityError.Code, identityError.Description);
        }
        if (identityError.Code.StartsWith("Invalid"))
        {
            return Error.Validation(identityError.Code, identityError.Description);
        }

        return Error.Internal("DatabaseFailure", "Server has failed action to perform on database");
    }
}
