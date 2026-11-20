namespace EquipmentBorrowing.Application.Interfaces;

public interface IUnitOfWork
{
    /// <summary>
    /// Runs the operation inside one database transaction: everything it writes
    /// is committed together or not at all.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
}