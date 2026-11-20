using EquipmentBorrowing.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EquipmentBorrowing.Infrastructure.Repositories;

internal static class SearchPattern
{
    public const string EscapeCharacter = "\\";

    public static string Contains(string text)
    {
        var escaped = text.Trim()
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
        return $"%{escaped}%";
    }
}

internal static class SaveChangesHelper
{
    public static async Task SaveAsync(DbContext context, CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            throw new DuplicateValueException(pg.ConstraintName ?? "unknown", ex);
        }
    }
}