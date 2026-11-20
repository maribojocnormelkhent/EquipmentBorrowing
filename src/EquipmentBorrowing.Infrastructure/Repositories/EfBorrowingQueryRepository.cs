using System.Linq.Expressions;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure.Repositories;

public class EfBorrowingQueryRepository : IBorrowingQueryRepository
{
    private sealed class Row
    {
        public Borrowing Borrowing { get; init; } = null!;
        public Student Student { get; init; } = null!;
        public Equipment Equipment { get; init; } = null!;
    }

    private static readonly Expression<Func<Row, BorrowingDetail>> ToDetail = x => new BorrowingDetail(
        x.Borrowing.Id,
        x.Student.Id,
        x.Student.StudentNumber,
        x.Student.Name,
        x.Equipment.Id,
        x.Equipment.Identifier,
        x.Equipment.Name,
        x.Borrowing.DateBorrowed,
        x.Borrowing.ExpectedReturnDate,
        x.Borrowing.ActualReturnDate,
        x.Borrowing.Status);

    private readonly EquipmentBorrowingDbContext _context;

    public EfBorrowingQueryRepository(EquipmentBorrowingDbContext context)
    {
        _context = context;
    }

    private IQueryable<Row> Rows() =>
        from b in _context.Borrowings.AsNoTracking()
        join s in _context.Students.AsNoTracking() on b.StudentId equals s.Id
        join e in _context.Equipment.AsNoTracking() on b.EquipmentId equals e.Id
        select new Row { Borrowing = b, Student = s, Equipment = e };

    public async Task<IReadOnlyList<BorrowingDetail>> SearchAsync(
        BorrowingSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = Rows();

        switch (criteria.Status)
        {
            case BorrowingStatusFilter.Active:
                query = query.Where(x => x.Borrowing.Status == BorrowingStatus.Active);
                break;
            case BorrowingStatusFilter.Returned:
                query = query.Where(x => x.Borrowing.Status == BorrowingStatus.Returned);
                break;
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            var pattern = SearchPattern.Contains(criteria.SearchText);
            query = query.Where(x =>
                EF.Functions.ILike(x.Student.Name, pattern, SearchPattern.EscapeCharacter) ||
                EF.Functions.ILike(x.Student.StudentNumber, pattern, SearchPattern.EscapeCharacter) ||
                EF.Functions.ILike(x.Equipment.Name, pattern, SearchPattern.EscapeCharacter));
        }

        var descending = criteria.Direction == SortDirection.Descending;

        IOrderedQueryable<Row> ordered = (criteria.SortBy, descending) switch
        {
            (BorrowingSortField.ExpectedReturnDate, false) => query.OrderBy(x => x.Borrowing.ExpectedReturnDate),
            (BorrowingSortField.ExpectedReturnDate, true) => query.OrderByDescending(x => x.Borrowing.ExpectedReturnDate),
            (_, false) => query.OrderBy(x => x.Borrowing.DateBorrowed),
            _ => query.OrderByDescending(x => x.Borrowing.DateBorrowed)
        };

        // Borrowing Id (ascending) is the stable tie-breaker.
        return await ordered
            .ThenBy(x => x.Borrowing.Id)
            .Select(ToDetail)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BorrowingDetail>> GetForStudentAsync(
        int studentId, CancellationToken cancellationToken = default)
    {
        return await Rows()
            .Where(x => x.Student.Id == studentId)
            .OrderByDescending(x => x.Borrowing.DateBorrowed)
            .ThenByDescending(x => x.Borrowing.Id)
            .Select(ToDetail)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BorrowingDetail>> GetForEquipmentAsync(
        int equipmentId, CancellationToken cancellationToken = default)
    {
        return await Rows()
            .Where(x => x.Equipment.Id == equipmentId)
            .OrderByDescending(x => x.Borrowing.DateBorrowed)
            .ThenByDescending(x => x.Borrowing.Id)
            .Select(ToDetail)
            .ToListAsync(cancellationToken);
    }
}