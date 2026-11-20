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

    public Task<IReadOnlyList<Student>> SearchAsync(string? searchText, CancellationToken cancellationToken = default)
    {
        IEnumerable<Student> query = _students;
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var text = searchText.Trim();
            query = query.Where(s =>
                s.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                s.StudentNumber.Contains(text, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult((IReadOnlyList<Student>)query.OrderBy(s => s.Name).ToList());
    }

    public Task<Student?> GetByStudentNumberAsync(string studentNumber, CancellationToken cancellationToken = default)
    {
        var key = studentNumber.Trim();
        return Task.FromResult(_students.FirstOrDefault(
            s => string.Equals(s.StudentNumber, key, StringComparison.OrdinalIgnoreCase)));
    }

    public Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        _students.Add(student);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        var index = _students.FindIndex(s => s.Id == student.Id);
        if (index >= 0)
            _students[index] = student;

        return Task.CompletedTask;
    }
}