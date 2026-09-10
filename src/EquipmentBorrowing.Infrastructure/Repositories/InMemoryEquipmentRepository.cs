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

    public Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        var index = _equipment.FindIndex(e => e.Id == equipment.Id);
        if (index >= 0)
            _equipment[index] = equipment;

        return Task.CompletedTask;
    }
}