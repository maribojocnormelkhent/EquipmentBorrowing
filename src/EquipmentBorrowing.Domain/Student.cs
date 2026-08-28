namespace EquipmentBorrowing.Domain;

/// <summary>
/// Represents a student who may borrow equipment.
///
/// Responsibility: hold identity information and whether the student is
/// currently allowed to borrow.
///
/// NOT the responsibility of this class: deciding how many items a student
/// currently has borrowed (that depends on Borrowing records, which live
/// outside this object), and NOT responsible for checking equipment
/// availability.
/// </summary>
public class Student
{
    public int Id { get; }
    public string Name { get; }
    public bool IsAllowedToBorrow { get; private set; }

    public Student(int id, string name, bool isAllowedToBorrow = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Student name is required.", nameof(name));

        Id = id;
        Name = name;
        IsAllowedToBorrow = isAllowedToBorrow;
    }

    public void Suspend() => IsAllowedToBorrow = false;

    public void Reinstate() => IsAllowedToBorrow = true;
}
