using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

/// <summary>
/// Abstraction over wherever Borrowing records actually live.
/// Only the operations the Borrow Equipment use case actually needs are
/// declared here — no speculative methods.
/// </summary>
public interface IBorrowingRepository
{
    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);

    Task<int> GetActiveBorrowingCountForStudentAsync(
        int studentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific borrowing by its id, needed so Return Equipment
    /// can find the record it is about to update.
    /// </summary>
    Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes made to an existing Borrowing instance
    /// (for example, after it has been marked as returned).
    /// </summary>
    Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default);
}

