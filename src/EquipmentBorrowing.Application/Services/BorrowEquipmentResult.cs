using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

/// <summary>
/// Outcome of attempting the Borrow Equipment use case. Using an explicit
/// result object (instead of throwing exceptions for expected business
/// failures such as "equipment unavailable") keeps failure handling simple
/// for whatever calls this service — console app today, an Avalonia
/// ViewModel later.
/// </summary>
public class BorrowEquipmentResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public Borrowing? Borrowing { get; }

    private BorrowEquipmentResult(bool success, string? errorMessage, Borrowing? borrowing)
    {
        Success = success;
        ErrorMessage = errorMessage;
        Borrowing = borrowing;
    }

    public static BorrowEquipmentResult Fail(string errorMessage) =>
        new(success: false, errorMessage, borrowing: null);

    public static BorrowEquipmentResult Ok(Borrowing borrowing) =>
        new(success: true, errorMessage: null, borrowing);
}
