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
        var equipment = _equipment.FirstOrDefault(e => e.Id == id);
        return Task.FromResult(equipment);
    }

    public Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        var index = _equipment.FindIndex(e => e.Id == equipment.Id);
        if (index >= 0)
            _equipment[index] = equipment;

        return Task.CompletedTask;
    }
}
