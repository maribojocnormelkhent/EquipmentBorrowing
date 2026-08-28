using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

/// <summary>
/// Simple in-memory implementation of IStudentRepository, using a
/// List&lt;Student&gt; as the "database". This class belongs in
/// Infrastructure because it is a technical storage detail — the
/// Application layer only knows about IStudentRepository.
/// </summary>
public class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Student> _students;

    public InMemoryStudentRepository(IEnumerable<Student>? seedData = null)
    {
        _students = seedData?.ToList() ?? new List<Student>();
    }

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(student);
    }
}
