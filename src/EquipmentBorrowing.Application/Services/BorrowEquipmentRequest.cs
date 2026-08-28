namespace EquipmentBorrowing.Application.Services;

/// <summary>
/// Input data for the Borrow Equipment use case.
/// </summary>
public record BorrowEquipmentRequest(
    int StudentId,
    int EquipmentId,
    DateTime ExpectedReturnDate);
