using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

/// <summary>
/// Outcome of attempting the Return Equipment use case.
/// </summary>
public class ReturnEquipmentResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public Borrowing? Borrowing { get; }

    private ReturnEquipmentResult(bool success, string? errorMessage, Borrowing? borrowing)
    {
        Success = success;
        ErrorMessage = errorMessage;
        Borrowing = borrowing;
    }

    public static ReturnEquipmentResult Fail(string errorMessage) =>
        new(success: false, errorMessage, borrowing: null);

    public static ReturnEquipmentResult Ok(Borrowing borrowing) =>
        new(success: true, errorMessage: null, borrowing);
}