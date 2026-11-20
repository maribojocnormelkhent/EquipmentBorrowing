using EquipmentBorrowing.Application.Services;

namespace EquipmentBorrowing.Application.Interfaces;

public interface IBorrowingQueryRepository
{
    Task<IReadOnlyList<BorrowingDetail>> SearchAsync(
        BorrowingSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BorrowingDetail>> GetForStudentAsync(
        int studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BorrowingDetail>> GetForEquipmentAsync(
        int equipmentId, CancellationToken cancellationToken = default);
}