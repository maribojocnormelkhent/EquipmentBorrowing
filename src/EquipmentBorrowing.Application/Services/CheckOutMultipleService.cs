using EquipmentBorrowing.Application.Exceptions;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using CheckOutResult = EquipmentBorrowing.Application.Services.OperationResult<System.Collections.Generic.IReadOnlyList<EquipmentBorrowing.Domain.Borrowing>>;

namespace EquipmentBorrowing.Application.Services;

public record CheckOutMultipleRequest(int StudentId, IReadOnlyList<int> EquipmentIds, DateTime ExpectedReturnDate);

public class CheckOutMultipleService
{
    private const int MaxActiveBorrowingsPerStudent = 3;
    private const int MinimumItems = 2;
    public const string FaultPointAfterFirstItem = "checkout-after-first-item";

    private readonly IStudentRepository _students;
    private readonly IEquipmentRepository _equipment;
    private readonly IBorrowingRepository _borrowings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFaultInjector _faults;

    public CheckOutMultipleService(
        IStudentRepository students,
        IEquipmentRepository equipment,
        IBorrowingRepository borrowings,
        IUnitOfWork unitOfWork,
        IFaultInjector faults)
    {
        _students = students;
        _equipment = equipment;
        _borrowings = borrowings;
        _unitOfWork = unitOfWork;
        _faults = faults;
    }

    public async Task<CheckOutResult> ExecuteAsync(
        CheckOutMultipleRequest request, CancellationToken cancellationToken = default)
    {
        var ids = request.EquipmentIds ?? Array.Empty<int>();

        if (ids.Count < MinimumItems)
            return CheckOutResult.Fail("Select at least two items for a multi-item checkout.");

        if (ids.Distinct().Count() != ids.Count)
            return CheckOutResult.Fail("The same item was selected more than once. Nothing was saved.");

        var checkoutDate = DateOnly.FromDateTime(DateTime.Now);
        var expectedReturnDate = DateOnly.FromDateTime(request.ExpectedReturnDate);
        if (expectedReturnDate <= checkoutDate)
            return CheckOutResult.Fail("The expected return date must be later than the checkout date (today).");

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(
                token => CheckOutAsync(request.StudentId, ids, checkoutDate, expectedReturnDate, token),
                cancellationToken);
        }
        catch (DuplicateValueException)
        {
            return CheckOutResult.Fail(
                "One of the items was just checked out by another operation. Nothing was saved. Reload and try again.");
        }
    }

    private async Task<CheckOutResult> CheckOutAsync(
        int studentId,
        IReadOnlyList<int> equipmentIds,
        DateOnly checkoutDate,
        DateOnly expectedReturnDate,
        CancellationToken cancellationToken)
    {
        // Everything below runs inside the transaction: current data is revalidated (BR-15).
        var student = await _students.GetByIdAsync(studentId, cancellationToken);
        if (student is null)
            return CheckOutResult.Fail($"Student {studentId} does not exist. Nothing was saved.");

        if (!student.IsAllowedToBorrow)
            return CheckOutResult.Fail($"Student '{student.Name}' is not currently allowed to borrow equipment.");

        var items = new List<Equipment>();
        foreach (var id in equipmentIds)
        {
            var item = await _equipment.GetByIdAsync(id, cancellationToken);
            if (item is null)
                return CheckOutResult.Fail($"Equipment {id} does not exist. Nothing was saved.");

            if (!item.IsAvailable)
                return CheckOutResult.Fail($"Equipment '{item.Name}' is not currently available. Nothing was saved.");

            items.Add(item);
        }

        var activeCount = await _borrowings.GetActiveBorrowingCountForStudentAsync(studentId, cancellationToken);
        if (activeCount + items.Count > MaxActiveBorrowingsPerStudent)
            return CheckOutResult.Fail(
                $"Student '{student.Name}' has {activeCount} active borrowing(s); adding {items.Count} more " +
                $"would exceed the limit of {MaxActiveBorrowingsPerStudent}. Nothing was saved.");

        var created = new List<Borrowing>();
        foreach (var item in items)
        {
            var borrowing = new Borrowing(0, studentId, item.Id, checkoutDate, expectedReturnDate);
            item.MarkBorrowed();

            await _equipment.UpdateAsync(item, cancellationToken);
            await _borrowings.AddAsync(borrowing, cancellationToken);
            created.Add(borrowing);

            // Demo seam: fails after the first item's writes, to prove the rollback.
            if (created.Count == 1)
                _faults.Check(FaultPointAfterFirstItem);
        }

        return CheckOutResult.Ok(created, $"Checked out {created.Count} items to '{student.Name}'.");
    }
}