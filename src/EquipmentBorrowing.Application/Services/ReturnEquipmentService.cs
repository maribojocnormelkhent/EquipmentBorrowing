using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

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
        var borrowing = await _borrowingRepository.GetByIdAsync(request.BorrowingId, cancellationToken);
        if (borrowing is null)
            return ReturnEquipmentResult.Fail($"Borrowing {request.BorrowingId} was not found.");

        if (borrowing.Status == BorrowingStatus.Returned)
            return ReturnEquipmentResult.Fail($"Borrowing {request.BorrowingId} has already been returned.");

        var equipment = await _equipmentRepository.GetByIdAsync(borrowing.EquipmentId, cancellationToken);
        if (equipment is null)
            return ReturnEquipmentResult.Fail($"Equipment {borrowing.EquipmentId} was not found.");

        borrowing.MarkReturned(DateOnly.FromDateTime(DateTime.Now));
        equipment.MarkReturned();

        await _borrowingRepository.UpdateAsync(borrowing, cancellationToken);
        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        return ReturnEquipmentResult.Ok(borrowing);
    }
}