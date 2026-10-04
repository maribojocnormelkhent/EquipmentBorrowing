using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;


public class BorrowEquipmentService
{
 
    private const int MaxActiveBorrowingsPerStudent = 3;

    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
    }

    public async Task<BorrowEquipmentResult> ExecuteAsync(
        BorrowEquipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Does the student exist?
        var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
            return BorrowEquipmentResult.Fail($"Student {request.StudentId} does not exist.");

        // 2. Is the student allowed to borrow?
        if (!student.IsAllowedToBorrow)
            return BorrowEquipmentResult.Fail($"Student '{student.Name}' is not currently allowed to borrow equipment.");

        // 3. Does the equipment exist?
        var equipment = await _equipmentRepository.GetByIdAsync(request.EquipmentId, cancellationToken);
        if (equipment is null)
            return BorrowEquipmentResult.Fail($"Equipment {request.EquipmentId} does not exist.");

        // 4. Is the equipment currently available?
        if (!equipment.IsAvailable)
            return BorrowEquipmentResult.Fail($"Equipment '{equipment.Name}' is not currently available.");

        // 5. Has the student reached the allowed number of active borrowings?
        var activeCount = await _borrowingRepository.GetActiveBorrowingCountForStudentAsync(
            request.StudentId, cancellationToken);

        if (activeCount >= MaxActiveBorrowingsPerStudent)
            return BorrowEquipmentResult.Fail(
                $"Student '{student.Name}' has reached the maximum of {MaxActiveBorrowingsPerStudent} active borrowings.");

        // 6. All rules satisfied — create the borrowing record.
        var borrowing = new Borrowing(
            id: 0,
            studentId: request.StudentId,
            equipmentId: request.EquipmentId,
            dateBorrowed: DateTime.Now,
            expectedReturnDate: request.ExpectedReturnDate);

        equipment.MarkBorrowed();

        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);
        await _borrowingRepository.AddAsync(borrowing, cancellationToken);

        return BorrowEquipmentResult.Ok(borrowing);
    }
}
