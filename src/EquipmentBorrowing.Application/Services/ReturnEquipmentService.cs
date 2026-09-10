using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

/// <summary>
/// Coordinates the "Return Equipment" use case. Like BorrowEquipmentService,
/// this class only depends on repository INTERFACES, received through the
/// constructor, and contains no database or UI code.
/// </summary>
public class ReturnEquipmentService
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    public ReturnEquipmentService(
        IBorrowingRepository borrowingRepository,
        IEquipmentRepository equipmentRepository)
    {
        _borrowingRepository = borrowingRepository;
        _equipmentRepository = equipmentRepository;
    }

    public async Task<ReturnEquipmentResult> ExecuteAsync(
        ReturnEquipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Does the borrowing record exist?
        var borrowing = await _borrowingRepository.GetByIdAsync(request.BorrowingId, cancellationToken);
        if (borrowing is null)
            return ReturnEquipmentResult.Fail($"Borrowing {request.BorrowingId} was not found.");

        // 2. Has it already been returned?
        if (borrowing.Status == BorrowingStatus.Returned)
            return ReturnEquipmentResult.Fail($"Borrowing {request.BorrowingId} has already been returned.");

        // 3. Does the linked equipment still exist?
        var equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId, cancellationToken);
        if (equipment is null)
            return ReturnEquipmentResult.Fail($"Equipment {borrowing.EquipmentId} was not found.");

        // 4. Apply the change: mark both objects, then persist both.
        borrowing.MarkReturned();
        equipment.MarkReturned();

        await _borrowingRepository.UpdateAsync(borrowing, cancellationToken);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        return ReturnEquipmentResult.Ok(borrowing);
    }
}