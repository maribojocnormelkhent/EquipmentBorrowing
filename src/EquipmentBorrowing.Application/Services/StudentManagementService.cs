using EquipmentBorrowing.Application.Exceptions;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Domain;

namespace EquipmentBorrowing.Application.Services;

public record RegisterStudentRequest(string? StudentNumber, string? Name, string? Contact);

public record UpdateStudentRequest(int StudentId, string? StudentNumber, string? Name, string? Contact);

public class StudentManagementService
{
    private const int MaxNumberLength = 50;
    private const int MaxNameLength = 150;
    private const int MaxContactLength = 150;

    private readonly IStudentRepository _students;
    private readonly IUnitOfWork _unitOfWork;

    public StudentManagementService(IStudentRepository students, IUnitOfWork unitOfWork)
    {
        _students = students;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<Student>> SearchAsync(
        string? searchText, CancellationToken cancellationToken = default)
        => _students.SearchAsync(searchText, cancellationToken);

    // UC-12
    public async Task<OperationResult<Student>> RegisterAsync(
        RegisterStudentRequest request, CancellationToken cancellationToken = default)
    {
        var error = Validate(request.StudentNumber, request.Name, request.Contact,
            out var number, out var name, out var contact);
        if (error is not null)
            return OperationResult<Student>.Fail(error);

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                if (await _students.GetByStudentNumberAsync(number, token) is not null)
                    return OperationResult<Student>.Fail(DuplicateMessage(number));

                var student = new Student(0, name, number, isAllowedToBorrow: true, contact: contact);
                await _students.AddAsync(student, token);
                return OperationResult<Student>.Ok(student, $"Student '{student.Name}' registered.");
            }, cancellationToken);
        }
        catch (DuplicateValueException)
        {
            return OperationResult<Student>.Fail(DuplicateMessage(number));
        }
    }

    // UC-13
    public async Task<OperationResult<Student>> UpdateAsync(
        UpdateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var error = Validate(request.StudentNumber, request.Name, request.Contact,
            out var number, out var name, out var contact);
        if (error is not null)
            return OperationResult<Student>.Fail(error);

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async token =>
            {
                var student = await _students.GetByIdAsync(request.StudentId, token);
                if (student is null)
                    return OperationResult<Student>.Fail(
                        "This student no longer exists. Reload the list and select again.");

                var other = await _students.GetByStudentNumberAsync(number, token);
                if (other is not null && other.Id != student.Id)
                    return OperationResult<Student>.Fail(DuplicateMessage(number));

                student.UpdateDetails(number, name, contact);
                await _students.UpdateAsync(student, token);
                return OperationResult<Student>.Ok(student, $"Student '{student.Name}' updated.");
            }, cancellationToken);
        }
        catch (DuplicateValueException)
        {
            return OperationResult<Student>.Fail(DuplicateMessage(number));
        }
    }

    // UC-14
    public Task<OperationResult<Student>> SetEligibilityAsync(
        int studentId, bool allowedToBorrow, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var student = await _students.GetByIdAsync(studentId, token);
            if (student is null)
                return OperationResult<Student>.Fail(
                    "This student no longer exists. Reload the list and select again.");

            if (student.IsAllowedToBorrow == allowedToBorrow)
                return OperationResult<Student>.Ok(student,
                    allowedToBorrow
                        ? $"'{student.Name}' is already permitted to borrow. No changes made."
                        : $"'{student.Name}' is already restricted from borrowing. No changes made.");

            if (allowedToBorrow)
                student.Reinstate();
            else
                student.Suspend();

            await _students.UpdateAsync(student, token);
            return OperationResult<Student>.Ok(student,
                allowedToBorrow
                    ? $"'{student.Name}' is now permitted to borrow."
                    : $"'{student.Name}' is now restricted. Existing borrowings are unchanged and can still be returned.");
        }, cancellationToken);
    }

    private static string DuplicateMessage(string number) =>
        $"A student with student number '{number}' already exists.";

    private static string? Validate(
        string? rawNumber, string? rawName, string? rawContact,
        out string number, out string name, out string? contact)
    {
        number = rawNumber?.Trim() ?? string.Empty;
        name = rawName?.Trim() ?? string.Empty;
        contact = string.IsNullOrWhiteSpace(rawContact) ? null : rawContact.Trim();

        if (number.Length == 0) return "Student number is required.";
        if (name.Length == 0) return "Full name is required.";
        if (number.Length > MaxNumberLength) return $"Student number must be at most {MaxNumberLength} characters.";
        if (name.Length > MaxNameLength) return $"Full name must be at most {MaxNameLength} characters.";
        if (contact is not null && contact.Length > MaxContactLength)
            return $"Contact must be at most {MaxContactLength} characters.";
        return null;
    }
}