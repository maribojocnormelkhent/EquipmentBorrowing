using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public class BorrowEquipmentService
{
    private const int MaxActiveBorrowingsPerStudent = 3;

    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BorrowEquipmentService(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IBorrowingRepository borrowingRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowingRepository = borrowingRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<BorrowEquipmentResult> ExecuteAsync(
        BorrowEquipmentRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validation AND writes happen inside one transaction, so the rules are
        // checked against current data at the transaction boundary (BR-15).
        return _unitOfWork.ExecuteInTransactionAsync(
            token => ExecuteCoreAsync(request, token), cancellationToken);
    }

    private async Task<BorrowEquipmentResult> ExecuteCoreAsync(
        BorrowEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
            return BorrowEquipmentResult.Fail($"Student {request.StudentId} does not exist.");

        if (!student.IsAllowedToBorrow)
            return BorrowEquipmentResult.Fail($"Student '{student.Name}' is not currently allowed to borrow equipment.");

        var equipment = await _equipmentRepository.GetByIdAsync(request.EquipmentId, cancellationToken);
        if (equipment is null)
            return BorrowEquipmentResult.Fail($"Equipment {request.EquipmentId} does not exist.");

        if (!equipment.IsAvailable)
            return BorrowEquipmentResult.Fail($"Equipment '{equipment.Name}' is not currently available.");

        // Dates are local calendar dates (BR-11): checkout is today.
        var checkoutDate = DateOnly.FromDateTime(DateTime.Now);
        var expectedReturnDate = DateOnly.FromDateTime(request.ExpectedReturnDate);
        if (expectedReturnDate <= checkoutDate)
            return BorrowEquipmentResult.Fail("The expected return date must be later than the checkout date (today).");

        var activeCount = await _borrowingRepository.GetActiveBorrowingCountForStudentAsync(
            request.StudentId, cancellationToken);

        if (activeCount >= MaxActiveBorrowingsPerStudent)
            return BorrowEquipmentResult.Fail(
                $"Student '{student.Name}' has reached the maximum of {MaxActiveBorrowingsPerStudent} active borrowings.");

        var borrowing = new Borrowing(
            id: 0,
            studentId: request.StudentId,
            equipmentId: request.EquipmentId,
            dateBorrowed: checkoutDate,
            expectedReturnDate: expectedReturnDate);

        equipment.MarkBorrowed();

        await _equipmentRepository.UpdateAsync(equipment, cancellationToken);
        await _borrowingRepository.AddAsync(borrowing, cancellationToken);

        return BorrowEquipmentResult.Ok(borrowing);
    }
}