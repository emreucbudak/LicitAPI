using Microsoft.EntityFrameworkCore;

namespace Licit.WalletService.Application.Extensions;

public static class DbUpdateExceptionExtensions
{
    private const string PostgresUniqueViolationSqlState = "23505";

    public static bool IsUniqueConstraintViolation(this DbUpdateException exception, string constraintName)
    {
        var databaseException = exception.InnerException;
        if (databaseException is null)
            return false;

        var sqlState = databaseException.GetType().GetProperty("SqlState")?.GetValue(databaseException) as string;
        if (!string.Equals(sqlState, PostgresUniqueViolationSqlState, StringComparison.Ordinal))
            return false;

        var violatedConstraint = databaseException.GetType().GetProperty("ConstraintName")?.GetValue(databaseException) as string;
        return string.Equals(violatedConstraint, constraintName, StringComparison.Ordinal);
    }
}
