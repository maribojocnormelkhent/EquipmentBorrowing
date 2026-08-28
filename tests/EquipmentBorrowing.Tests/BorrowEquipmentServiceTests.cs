using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class BorrowEquipmentServiceTests
{
    private static BorrowEquipmentService CreateService(
        out InMemoryEquipmentRepository equipmentRepository,
        out InMemoryBorrowingRepository borrowingRepository)
    {
        var studentRepository = new InMemoryStudentRepository(new[]
        {
            new Student(1, "Allowed Student", isAllowedToBorrow: true),
            new Student(2, "Suspended Student", isAllowedToBorrow: false),
        });

        equipmentRepository = new InMemoryEquipmentRepository(new[]
        {
            new Equipment(100, "Multimeter", isAvailable: true),
            new Equipment(101, "Oscilloscope", isAvailable: false),
        });

        borrowingRepository = new InMemoryBorrowingRepository();

        return new BorrowEquipmentService(studentRepository, equipmentRepository, borrowingRepository);
    }

    [Fact]
    public async Task Borrow_Succeeds_When_Student_And_Equipment_Are_Valid()
    {
        var service = CreateService(out _, out _);

        var result = await service.ExecuteAsync(new BorrowEquipmentRequest(1, 100, DateTime.Now.AddDays(7)));

        Assert.True(result.Success);
        Assert.NotNull(result.Borrowing);
        Assert.Equal(BorrowingStatus.Active, result.Borrowing!.Status);
    }

    [Fact]
    public async Task Borrow_Fails_When_Equipment_Is_Unavailable()
    {
        var service = CreateService(out _, out _);

        var result = await service.ExecuteAsync(new BorrowEquipmentRequest(1, 101, DateTime.Now.AddDays(7)));

        Assert.False(result.Success);
        Assert.Null(result.Borrowing);
    }

    [Fact]
    public async Task Borrow_Fails_When_Student_Is_Not_Allowed_To_Borrow()
    {
        var service = CreateService(out _, out _);

        var result = await service.ExecuteAsync(new BorrowEquipmentRequest(2, 100, DateTime.Now.AddDays(7)));

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Borrow_Fails_When_Equipment_Does_Not_Exist()
    {
        var service = CreateService(out _, out _);

        var result = await service.ExecuteAsync(new BorrowEquipmentRequest(1, 999, DateTime.Now.AddDays(7)));

        Assert.False(result.Success);
    }
}
