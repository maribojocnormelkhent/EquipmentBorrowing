using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryEquipmentRepository : IEquipmentRepository
{
    private readonly List<Equipment> _equipment;

    public InMemoryEquipmentRepository(IEnumerable<Equipment>? seedData = null)
    {
        _equipment = seedData?.ToList() ?? new List<Equipment>();
    }

    public Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_equipment.FirstOrDefault(e => e.Id == id));
    }

    public Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<Equipment>)_equipment.ToList());
    }

    public Task<IReadOnlyList<Equipment>> SearchAsync(
        string? searchText, bool availableOnly, CancellationToken cancellationToken = default)
    {
        IEnumerable<Equipment> query = _equipment;
        if (availableOnly)
            query = query.Where(e => e.IsAvailable);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var text = searchText.Trim();
            query = query.Where(e =>
                e.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                e.Identifier.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                (e.Description?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return Task.FromResult((IReadOnlyList<Equipment>)query.OrderBy(e => e.Name).ToList());
    }

    public Task<Equipment?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var key = identifier.Trim();
        return Task.FromResult(_equipment.FirstOrDefault(
            e => string.Equals(e.Identifier, key, StringComparison.OrdinalIgnoreCase)));
    }

    public Task AddAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        _equipment.Add(equipment);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        var index = _equipment.FindIndex(e => e.Id == equipment.Id);
        if (index >= 0)
            _equipment[index] = equipment;

        return Task.CompletedTask;
    }
}