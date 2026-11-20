using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfEquipmentRepository : IEquipmentRepository
{
    private readonly EquipmentBorrowingDbContext _context;

    public EfEquipmentRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    public async Task<Equipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Equipment
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Equipment
            .AsNoTracking()
            .OrderBy(e => e.Name).ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> SearchAsync(
        string? searchText, bool availableOnly, CancellationToken cancellationToken = default)
    {
        var query = _context.Equipment.AsNoTracking();

        if (availableOnly)
            query = query.Where(e => e.IsAvailable);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var pattern = SearchPattern.Contains(searchText);
            query = query.Where(e =>
                EF.Functions.ILike(e.Name, pattern, SearchPattern.EscapeCharacter) ||
                EF.Functions.ILike(e.Identifier, pattern, SearchPattern.EscapeCharacter) ||
                (e.Description != null &&
                 EF.Functions.ILike(e.Description, pattern, SearchPattern.EscapeCharacter)));
        }

        return await query.OrderBy(e => e.Name).ThenBy(e => e.Id).ToListAsync(cancellationToken);
    }

    public async Task<Equipment?> GetByIdentifierAsync(
        string identifier, CancellationToken cancellationToken = default)
    {
        var key = identifier.Trim().ToLower();
        return await _context.Equipment
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Identifier.ToLower() == key, cancellationToken);
    }

    public async Task AddAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        await _context.Equipment.AddAsync(equipment, cancellationToken);
        await SaveChangesHelper.SaveAsync(_context, cancellationToken);
    }

    public async Task UpdateAsync(Equipment equipment, CancellationToken cancellationToken = default)
    {
        _context.Equipment.Update(equipment);
        await SaveChangesHelper.SaveAsync(_context, cancellationToken);
    }
}