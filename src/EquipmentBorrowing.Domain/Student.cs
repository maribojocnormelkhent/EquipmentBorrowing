namespace EquipmentBorrowing.Domain;

public class Student
{
    public int Id { get; }
    public string StudentNumber { get; private set; }
    public string Name { get; private set; }
    public string? Contact { get; private set; }
    public bool IsAllowedToBorrow { get; private set; }

    public Student(int id, string name, string studentNumber, bool isAllowedToBorrow = true, string? contact = null)
    {
        Id = id;
        StudentNumber = Required(studentNumber, "Student number is required.", nameof(studentNumber));
        Name = Required(name, "Student name is required.", nameof(name));
        Contact = Optional(contact);
        IsAllowedToBorrow = isAllowedToBorrow;
    }

    public void UpdateDetails(string studentNumber, string name, string? contact)
    {
        StudentNumber = Required(studentNumber, "Student number is required.", nameof(studentNumber));
        Name = Required(name, "Student name is required.", nameof(name));
        Contact = Optional(contact);
    }

    public void Suspend() => IsAllowedToBorrow = false;

    public void Reinstate() => IsAllowedToBorrow = true;

    private static string Required(string? value, string message, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(message, paramName);
        return value.Trim();
    }

    private static string? Optional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}