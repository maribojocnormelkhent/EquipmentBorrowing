using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Xunit;

namespace EquipmentBorrowing.Tests;

public class ReturnEquipmentServiceTests
{
    private static async Task<(ReturnEquipmentService service, Borrowing activeBorrowing)> CreateServiceWithActiveBorrowingAsync()
    {
        var studentRepository = new InMemoryStudentRepository(new[]
        {
            new Student(1, "Allowed Student", isAllowedToBorrow: true),
        });

        var equipmentRepository = new InMemoryEquipmentRepository(new[]
        {
            new Equipment(100, "Multimeter", isAvailable: true),
        });

        var borrowingRepository = new InMemoryBorrowingRepository();

        // Use the real Borrow flow to set up a genuine active borrowing,
        // instead of hand-constructing one — this keeps the test honest
        // about how a Borrowing actually gets created.
        var borrowService = new BorrowEquipmentService(studentRepository, equipmentRepository, borrowingRepository);
        var borrowResult = await borrowService.ExecuteAsync(
            new BorrowEquipmentRequest(1, 100, DateTime.Now.AddDays(7)));

        var returnService = new ReturnEquipmentService(borrowingRepository, equipmentRepository);

        return (returnService, borrowResult.Borrowing!);
    }

    [Fact]
    public async Task Return_Succeeds_When_Borrowing_Is_Active()
    {
        var (service, activeBorrowing) = await CreateServiceWithActiveBorrowingAsync();

        var result = await service.ExecuteAsync(new ReturnEquipmentRequest(activeBorrowing.Id));

        Assert.True(result.Success);
        Assert.NotNull(result.Borrowing);
        Assert.Equal(BorrowingStatus.Returned, result.Borrowing!.Status);
    }

    [Fact]
    public async Task Return_Fails_When_Borrowing_Already_Returned()
    {
        var (service, activeBorrowing) = await CreateServiceWithActiveBorrowingAsync();

        // Return it once — should succeed.
        await service.ExecuteAsync(new ReturnEquipmentRequest(activeBorrowing.Id));

        // Try returning the same borrowing a second time — should fail.
        var secondAttempt = await service.ExecuteAsync(new ReturnEquipmentRequest(activeBorrowing.Id));

        Assert.False(secondAttempt.Success);
    }

    [Fact]
    public async Task Return_Fails_When_Borrowing_Does_Not_Exist()
    {
        var (service, _) = await CreateServiceWithActiveBorrowingAsync();

        var result = await service.ExecuteAsync(new ReturnEquipmentRequest(999999));

        Assert.False(result.Success);
    }
}