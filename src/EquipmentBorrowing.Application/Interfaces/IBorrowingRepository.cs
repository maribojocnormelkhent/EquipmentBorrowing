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
}
