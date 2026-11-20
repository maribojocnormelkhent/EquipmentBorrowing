using EquipmentBorrowing.Application.Interfaces;
using HistoryResult = EquipmentBorrowing.Application.Services.OperationResult<System.Collections.Generic.IReadOnlyList<EquipmentBorrowing.Application.Services.BorrowingDetail>>;

namespace EquipmentBorrowing.Application.Services;

public class BorrowingQueryService
{
    private readonly IBorrowingQueryRepository _queries;
    private readonly IStudentRepository _students;
    private readonly IEquipmentRepository _equipment;

    public BorrowingQueryService(
        IBorrowingQueryRepository queries,
        IStudentRepository students,
        IEquipmentRepository equipment)
    {
        _queries = queries;
        _students = students;
        _equipment = equipment;
    }

    // UC-09
    public Task<IReadOnlyList<BorrowingDetail>> SearchAsync(
        BorrowingSearchCriteria criteria, CancellationToken cancellationToken = default)
        => _queries.SearchAsync(criteria, cancellationToken);

    // UC-04
    public Task<IReadOnlyList<BorrowingDetail>> GetActiveAsync(CancellationToken cancellationToken = default)
        => _queries.SearchAsync(
            new BorrowingSearchCriteria(
                null, BorrowingStatusFilter.Active, BorrowingSortField.ExpectedReturnDate, SortDirection.Ascending),
            cancellationToken);

    // UC-07
    public async Task<HistoryResult> GetStudentHistoryAsync(
        int studentId, CancellationToken cancellationToken = default)
    {
        var student = await _students.GetByIdAsync(studentId, cancellationToken);
        if (student is null)
            return HistoryResult.Fail("This student no longer exists. Reload and select again.");

        var records = await _queries.GetForStudentAsync(studentId, cancellationToken);
        return HistoryResult.Ok(records, records.Count == 0
            ? $"'{student.Name}' has no borrowing history yet."
            : $"{records.Count} record(s) for '{student.Name}'.");
    }

    // UC-08
    public async Task<HistoryResult> GetEquipmentHistoryAsync(
        int equipmentId, CancellationToken cancellationToken = default)
    {
        var item = await _equipment.GetByIdAsync(equipmentId, cancellationToken);
        if (item is null)
            return HistoryResult.Fail("This equipment no longer exists. Reload and select again.");

        var records = await _queries.GetForEquipmentAsync(equipmentId, cancellationToken);
        return HistoryResult.Ok(records, records.Count == 0
            ? $"'{item.Name}' has no borrowing history yet."
            : $"{records.Count} record(s) for '{item.Name}'.");
    }
}