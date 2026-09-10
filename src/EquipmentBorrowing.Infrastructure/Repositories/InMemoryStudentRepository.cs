using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Student> _students;

    public InMemoryStudentRepository(IEnumerable<Student>? seedData = null)
    {
        _students = seedData?.ToList() ?? new List<Student>();
    }

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_students.FirstOrDefault(s => s.Id == id));
    }

    public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<Student>)_students.ToList());
    }
}