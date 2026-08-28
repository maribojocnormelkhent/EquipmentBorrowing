namespace EquipmentBorrowing.Domain;

/// <summary>
/// Represents a single borrowing transaction: a student borrowing a specific
/// piece of equipment for a period of time.
///
/// Responsibility: hold the facts of the transaction and its current status,
/// and allow itself to be marked as returned.
///
/// NOT the responsibility of this class: deciding whether a new borrowing is
/// allowed to be created in the first place — that is a business rule that
/// belongs to the application service (BorrowEquipmentService), because it
/// needs information from multiple objects (Student, Equipment, and other
/// Borrowing records) to make that decision.
/// </summary>
public class Borrowing
{
    public int Id { get; }
    public int StudentId { get; }
    public int EquipmentId { get; }
    public DateTime DateBorrowed { get; }
    public DateTime ExpectedReturnDate { get; }
    public BorrowingStatus Status { get; private set; }

    public Borrowing(
        int id,
        int studentId,
        int equipmentId,
        DateTime dateBorrowed,
        DateTime expectedReturnDate)
    {
        if (expectedReturnDate < dateBorrowed)
            throw new ArgumentException("Expected return date cannot be before the borrow date.");

        Id = id;
        StudentId = studentId;
        EquipmentId = equipmentId;
        DateBorrowed = dateBorrowed;
        ExpectedReturnDate = expectedReturnDate;
        Status = BorrowingStatus.Active;
    }

    public void MarkReturned()
    {
        Status = BorrowingStatus.Returned;
    }
}
