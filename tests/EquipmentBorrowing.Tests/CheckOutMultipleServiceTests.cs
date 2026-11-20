using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class CheckOutMultipleServiceTests
{
    private static async Task<(CheckOutMultipleService Service, InMemoryEquipmentRepository Equipment, InMemoryBorrowingRepository Borrowings)>
        CreateAsync(int existingActiveForStudent1 = 0)
    {
        var students = new InMemoryStudentRepository(new[]
        {
            new Student(1, "Allowed Student", "S-001", isAllowedToBorrow: true),
            new Student(2, "Suspended Student", "S-002", isAllowedToBorrow: false),
        });

        var equipment = new InMemoryEquipmentRepository(new[]
        {
            new Equipment(100, "Multimeter", "EQ-100", isAvailable: true),
            new Equipment(101, "Oscilloscope", "EQ-101", isAvailable: true),
            new Equipment(102, "Generator", "EQ-102", isAvailable: true),
            new Equipment(103, "Soldering", "EQ-103", isAvailable: true),
            new Equipment(104, "Borrowed Item", "EQ-104", isAvailable: false),
        });

        var borrowings = new InMemoryBorrowingRepository();
        var today = DateOnly.FromDateTime(DateTime.Now);
        for (var i = 0; i < existingActiveForStudent1; i++)
            await borrowings.AddAsync(new Borrowing(0, 1, 900 + i, today, today.AddDays(7)));

        var service = new CheckOutMultipleService(
            students, equipment, borrowings, new InMemoryUnitOfWork(), new NoFaultInjector());

        return (service, equipment, borrowings);
    }

    private static CheckOutMultipleRequest Request(int studentId, params int[] ids) =>
        new(studentId, ids, DateTime.Now.AddDays(7));

    [Fact]
    public async Task CheckOut_Succeeds_For_Two_Available_Items()
    {
        var (service, equipment, _) = await CreateAsync();

        var result = await service.ExecuteAsync(Request(1, 100, 101));

        Assert.True(result.Success);
        Assert.Equal(2, result.Value!.Count);
        Assert.False((await equipment.GetByIdAsync(100))!.IsAvailable);
        Assert.False((await equipment.GetByIdAsync(101))!.IsAvailable);
    }

    [Fact]
    public async Task CheckOut_Rejects_Fewer_Than_Two_Items()
    {
        var (service, _, _) = await CreateAsync();

        var result = await service.ExecuteAsync(Request(1, 100));

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CheckOut_Rejects_Duplicate_Items_And_Changes_Nothing()
    {
        var (service, equipment, _) = await CreateAsync();

        var result = await service.ExecuteAsync(Request(1, 100, 100));

        Assert.False(result.Success);
        Assert.True((await equipment.GetByIdAsync(100))!.IsAvailable);
    }

    [Fact]
    public async Task CheckOut_Rejects_Unavailable_Item_And_Changes_Nothing()
    {
        var (service, equipment, _) = await CreateAsync();

        var result = await service.ExecuteAsync(Request(1, 100, 104));

        Assert.False(result.Success);
        Assert.True((await equipment.GetByIdAsync(100))!.IsAvailable);
    }

    [Fact]
    public async Task CheckOut_Rejects_Missing_Item_And_Changes_Nothing()
    {
        var (service, equipment, _) = await CreateAsync();

        var result = await service.ExecuteAsync(Request(1, 100, 999));

        Assert.False(result.Success);
        Assert.True((await equipment.GetByIdAsync(100))!.IsAvailable);
    }

    [Fact]
    public async Task CheckOut_Rejects_Ineligible_Student()
    {
        var (service, equipment, _) = await CreateAsync();

        var result = await service.ExecuteAsync(Request(2, 100, 101));

        Assert.False(result.Success);
        Assert.True((await equipment.GetByIdAsync(100))!.IsAvailable);
    }

    [Fact]
    public async Task CheckOut_Rejects_When_Limit_Would_Be_Exceeded()
    {
        var (service, equipment, _) = await CreateAsync(existingActiveForStudent1: 2);

        var result = await service.ExecuteAsync(Request(1, 100, 101));

        Assert.False(result.Success);
        Assert.True((await equipment.GetByIdAsync(100))!.IsAvailable);
    }

    [Fact]
    public async Task CheckOut_Succeeds_Exactly_At_The_Limit()
    {
        var (service, _, borrowings) = await CreateAsync(existingActiveForStudent1: 1);

        var result = await service.ExecuteAsync(Request(1, 100, 101));

        Assert.True(result.Success);
        Assert.Equal(3, await borrowings.GetActiveBorrowingCountForStudentAsync(1));
    }

    [Fact]
    public async Task CheckOut_Rejects_Return_Date_That_Is_Not_After_Today()
    {
        var (service, _, _) = await CreateAsync();

        var result = await service.ExecuteAsync(new CheckOutMultipleRequest(1, new[] { 100, 101 }, DateTime.Now));

        Assert.False(result.Success);
    }
}