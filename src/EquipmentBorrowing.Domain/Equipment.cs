namespace EquipmentBorrowing.Domain;

public class Equipment
{
    public int Id { get; }
    public string Identifier { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsAvailable { get; private set; }

    public Equipment(int id, string name, string identifier, bool isAvailable = true, string? description = null)
    {
        Id = id;
        Identifier = Required(identifier, "Equipment identifier is required.", nameof(identifier));
        Name = Required(name, "Equipment name is required.", nameof(name));
        Description = Optional(description);
        IsAvailable = isAvailable;
    }

    public void UpdateDetails(string identifier, string name, string? description)
    {
        Identifier = Required(identifier, "Equipment identifier is required.", nameof(identifier));
        Name = Required(name, "Equipment name is required.", nameof(name));
        Description = Optional(description);
    }

    public void MarkBorrowed() => IsAvailable = false;

    public void MarkReturned() => IsAvailable = true;

    private static string Required(string? value, string message, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(message, paramName);
        return value.Trim();
    }

    private static string? Optional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}