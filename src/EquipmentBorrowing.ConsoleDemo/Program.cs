using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;

// This executable's only job is to WIRE UP the dependencies (manual
// dependency injection) and demonstrate the use case. It contains no
// business rules of its own — all of that lives in BorrowEquipmentService.

var students = new List<Student>
{
    new Student(id: 1, name: "Juan Dela Cruz", isAllowedToBorrow: true),
    new Student(id: 2, name: "Maria Santos", isAllowedToBorrow: false), // suspended student
};

var equipment = new List<Equipment>
{
    new Equipment(id: 100, name: "Digital Multimeter", isAvailable: true),
    new Equipment(id: 101, name: "Oscilloscope", isAvailable: false), // already borrowed
};

var studentRepository = new InMemoryStudentRepository(students);
var equipmentRepository = new InMemoryEquipmentRepository(equipment);
var borrowingRepository = new InMemoryBorrowingRepository();

var service = new BorrowEquipmentService(
    studentRepository,
    equipmentRepository,
    borrowingRepository);

Console.WriteLine("=== Campus Equipment Borrowing System — Demonstration ===\n");

// ---------------------------------------------------------------
// SUCCESSFUL CASE: allowed student borrows available equipment
// ---------------------------------------------------------------
Console.WriteLine("--- Case 1: Successful borrow ---");
var successResult = await service.ExecuteAsync(new BorrowEquipmentRequest(
    StudentId: 1,
    EquipmentId: 100,
    ExpectedReturnDate: DateTime.Now.AddDays(7)));

PrintResult(successResult);
// ---------------------------------------------------------------
// NEW CASE: successful return of the equipment just borrowed
// ---------------------------------------------------------------
Console.WriteLine("\n--- Case 1b: Successful return ---");
var returnService = new ReturnEquipmentService(borrowingRepository, equipmentRepository);

if (successResult.Borrowing is not null)
{
    var returnResult = await returnService.ExecuteAsync(
        new ReturnEquipmentRequest(successResult.Borrowing.Id));

    if (returnResult.Success)
        Console.WriteLine($"SUCCESS: Borrowing #{returnResult.Borrowing!.Id} marked as {returnResult.Borrowing.Status}.");
    else
        Console.WriteLine($"FAILED: {returnResult.ErrorMessage}");
}

// Try returning the same borrowing again — should fail
Console.WriteLine("\n--- Case 1c: Failure — already returned ---");
if (successResult.Borrowing is not null)
{
    var duplicateReturn = await returnService.ExecuteAsync(
        new ReturnEquipmentRequest(successResult.Borrowing.Id));

    Console.WriteLine(duplicateReturn.Success
        ? "Unexpected success"
        : $"FAILED: {duplicateReturn.ErrorMessage}");
}

// ---------------------------------------------------------------
// FAILURE CASE 1: equipment already unavailable
// ---------------------------------------------------------------
Console.WriteLine("\n--- Case 2: Failure — equipment unavailable ---");
var failResult1 = await service.ExecuteAsync(new BorrowEquipmentRequest(
    StudentId: 1,
    EquipmentId: 101,
    ExpectedReturnDate: DateTime.Now.AddDays(7)));

PrintResult(failResult1);

// ---------------------------------------------------------------
// FAILURE CASE 2: student is not allowed to borrow
// ---------------------------------------------------------------
Console.WriteLine("\n--- Case 3: Failure — student not allowed to borrow ---");
var failResult2 = await service.ExecuteAsync(new BorrowEquipmentRequest(
    StudentId: 2,
    EquipmentId: 100,
    ExpectedReturnDate: DateTime.Now.AddDays(7)));

PrintResult(failResult2);

// ---------------------------------------------------------------
// FAILURE CASE 3: equipment does not exist
// ---------------------------------------------------------------
Console.WriteLine("\n--- Case 4: Failure — equipment does not exist ---");
var failResult3 = await service.ExecuteAsync(new BorrowEquipmentRequest(
    StudentId: 1,
    EquipmentId: 999,
    ExpectedReturnDate: DateTime.Now.AddDays(7)));

PrintResult(failResult3);

static void PrintResult(BorrowEquipmentResult result)
{
    if (result.Success && result.Borrowing is not null)
    {
        Console.WriteLine($"SUCCESS: Borrowing #{result.Borrowing.Id} created.");
        Console.WriteLine($"  Student ID:   {result.Borrowing.StudentId}");
        Console.WriteLine($"  Equipment ID: {result.Borrowing.EquipmentId}");
        Console.WriteLine($"  Borrowed:     {result.Borrowing.DateBorrowed:yyyy-MM-dd}");
        Console.WriteLine($"  Due:          {result.Borrowing.ExpectedReturnDate:yyyy-MM-dd}");
        Console.WriteLine($"  Status:       {result.Borrowing.Status}");
    }
    else
    {
        Console.WriteLine($"FAILED: {result.ErrorMessage}");
    }
}
