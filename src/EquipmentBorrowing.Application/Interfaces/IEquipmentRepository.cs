using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

/// <summary>
/// Abstraction over wherever Equipment data actually lives.
/// </summary>
public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes made to an existing Equipment instance
    /// (for example, after it has been marked as borrowed).
    /// </summary>
    Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default);
}
