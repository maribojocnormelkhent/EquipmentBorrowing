using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Interfaces;

/// <summary>
/// Abstraction over wherever Student data actually lives (in-memory today,
/// possibly SQLite later). The application layer only depends on this
/// interface, never on a concrete storage technology.
/// </summary>
public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
