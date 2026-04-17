using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BlogAPI.Infrastructure.Helper;

public static class DatabaseExceptionHelper
{
    public static bool IsUniqueConstraintViolation(
        DbUpdateException exception,
        string constraintName
        )
    {
        if ( exception.InnerException == null)
        {
            return false;
        }
        if (exception.InnerException is PostgresException psqlException)
        {
            return psqlException.SqlState == PostgresErrorCodes.UniqueViolation &&
                   psqlException.ConstraintName == constraintName;
        }

        return false;
    }
}
