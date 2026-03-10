using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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
        if (exception.InnerException is SqlException sqlException)
        {
            if (sqlException.Number == 2601 || sqlException.Number == 2627)
            {
                return sqlException.Message.Contains(constraintName);
            }
        }

        return false;
    }
}
