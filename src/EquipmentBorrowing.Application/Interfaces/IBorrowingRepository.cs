using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingRepository
{
    Task AddAsync(Borrowing borrowing, CancellationToken cancellationToken = default);

    Task<int> GetActiveBorrowingCountForStudentAsync(
        int studentId, CancellationToken cancellationToken = default);

    Task<Borrowing?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task UpdateAsync(Borrowing borrowing, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Borrowing>> GetActiveBorrowingsAsync(CancellationToken cancellationToken = default);
}