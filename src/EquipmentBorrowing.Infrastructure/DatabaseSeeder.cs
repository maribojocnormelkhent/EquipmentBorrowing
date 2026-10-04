using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(EquipmentBorrowingDbContext context)
    {
        if (await context.Students.AnyAsync())
            return;

        context.Students.AddRange(
            new Student(1, "Juan Dela Cruz", isAllowedToBorrow: true),
            new Student(2, "Maria Santos", isAllowedToBorrow: false));

        context.Equipment.AddRange(
            new Equipment(100, "Digital Multimeter", isAvailable: true),
            new Equipment(101, "Oscilloscope", isAvailable: true),
            new Equipment(102, "Function Generator", isAvailable: true));

        await context.SaveChangesAsync();
    }
}