namespace EquipmentBorrowing.Domain;

/// <summary>
/// Represents a piece of borrowable laboratory equipment.
///
/// Responsibility: identity information and current availability.
///
/// NOT the responsibility of this class: knowing who currently has it, or
/// enforcing borrowing limits — that belongs to the Borrowing model and the
/// application service that coordinates the use case.
/// </summary>
public class Equipment
{
    public int Id { get; }
    public string Name { get; }
    public bool IsAvailable { get; private set; }

    public Equipment(int id, string name, bool isAvailable = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Equipment name is required.", nameof(name));

        Id = id;
        Name = name;
        IsAvailable = isAvailable;
    }

    public void MarkBorrowed() => IsAvailable = false;

    public void MarkReturned() => IsAvailable = true;
}
