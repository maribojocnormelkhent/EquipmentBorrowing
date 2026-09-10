namespace EquipmentBorrowing.Application.Services;

/// <summary>
/// Input data for the Return Equipment use case.
/// </summary>
public record ReturnEquipmentRequest(int BorrowingId);