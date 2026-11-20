using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfStudentRepository : IStudentRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfStudentRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .AsNoTracking()
            .OrderBy(s => s.Name).ThenBy(s => s.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Student>> SearchAsync(
        string? searchText, CancellationToken cancellationToken = default)
    {
        var query = _context.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var pattern = SearchPattern.Contains(searchText);
            query = query.Where(s =>
                EF.Functions.ILike(s.Name, pattern, SearchPattern.EscapeCharacter) ||
                EF.Functions.ILike(s.StudentNumber, pattern, SearchPattern.EscapeCharacter));
        }

        return await query.OrderBy(s => s.Name).ThenBy(s => s.Id).ToListAsync(cancellationToken);
    }

    public async Task<Student?> GetByStudentNumberAsync(
        string studentNumber, CancellationToken cancellationToken = default)
    {
        var key = studentNumber.Trim().ToLower();
        return await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentNumber.ToLower() == key, cancellationToken);
    }

    public async Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        await _context.Students.AddAsync(student, cancellationToken);
        await SaveChangesHelper.SaveAsync(_context, cancellationToken);
    }

    public async Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        _context.Students.Update(student);
        await SaveChangesHelper.SaveAsync(_context, cancellationToken);
    }
}