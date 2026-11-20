namespace EquipmentBorrowing.Application.Exceptions;

/// <summary>A known failure. The transaction was rolled back and nothing was saved.</summary>
public class PersistenceException : Exception
{
    public PersistenceException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}

/// <summary>The connection failed during commit: it is NOT known whether the changes were saved.</summary>
public class CommitOutcomeUnknownException : Exception
{
    public CommitOutcomeUnknownException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}