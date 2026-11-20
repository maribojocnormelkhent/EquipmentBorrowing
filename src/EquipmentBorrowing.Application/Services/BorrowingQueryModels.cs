using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public enum BorrowingStatusFilter { All, Active, Returned }

public enum BorrowingSortField { CheckoutDate, ExpectedReturnDate }

public enum SortDirection { Ascending, Descending }

public record BorrowingSearchCriteria(
    string? SearchText = null,
    BorrowingStatusFilter Status = BorrowingStatusFilter.All,
    BorrowingSortField SortBy = BorrowingSortField.CheckoutDate,
    SortDirection Direction = SortDirection.Descending);

public record BorrowingDetail(
    int Id,
    int StudentId,
    string StudentNumber,
    string StudentName,
    int EquipmentId,
    string EquipmentIdentifier,
    string EquipmentName,
    DateOnly DateBorrowed,
    DateOnly ExpectedReturnDate,
    DateOnly? ActualReturnDate,
    BorrowingStatus Status);