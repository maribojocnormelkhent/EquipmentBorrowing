namespace EquipmentBorrowing.Application.Exceptions;

/// <summary>A PostgreSQL unique constraint rejected the write (e.g. a concurrent duplicate).</summary>
public class DuplicateValueException : Exception
{
    public string ConstraintName { get; }

    public DuplicateValueException(string constraintName, Exception? innerException = null)
        : base($"Duplicate value violates constraint '{constraintName}'.", innerException)
    {
        ConstraintName = constraintName;
    }
}