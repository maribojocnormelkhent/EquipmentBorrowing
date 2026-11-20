using EquipmentBorrowing.Application.Exceptions;
using EquipmentBorrowing.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfUnitOfWork(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction;
        try
        {
            transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new PersistenceException("Could not reach the database. No changes were made.", ex);
        }

        await using (transaction)
        {
            T result;
            try
            {
                result = await operation(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await TryRollbackAsync(transaction);
                _context.ChangeTracker.Clear();
                throw new PersistenceException(
                    "The operation failed and was rolled back. No changes were saved.", ex);
            }

            try
            {
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _context.ChangeTracker.Clear();
                throw new CommitOutcomeUnknownException(
                    "The connection was lost while saving. It is not known whether the changes were saved. " +
                    "Reload the data and check before trying again.", ex);
            }

            return result;
        }
    }

    private static async Task TryRollbackAsync(IDbContextTransaction transaction)
    {
        try
        {
            await transaction.RollbackAsync();
        }
        catch
        {
            // The connection may already be gone; the server discards uncommitted work.
        }
    }
}