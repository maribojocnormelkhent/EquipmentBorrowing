using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowing.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(EquipmentBorrowingDbContext context)
    {
        if (await context.Students.AnyAsync() || await context.Equipment.AnyAsync())
            return;

        context.Students.AddRange(
            new Student(0, "Juan Dela Cruz", "2024-0001", isAllowedToBorrow: true, contact: "juan@example.com"),
            new Student(0, "Maria Santos", "2024-0002", isAllowedToBorrow: false),
            new Student(0, "Pedro Reyes", "2024-0003", isAllowedToBorrow: true));

        context.Equipment.AddRange(
            new Equipment(0, "Digital Multimeter", "EQ-001", isAvailable: true, description: "Handheld multimeter"),
            new Equipment(0, "Oscilloscope", "EQ-002", isAvailable: true, description: "2-channel oscilloscope"),
            new Equipment(0, "Function Generator", "EQ-003", isAvailable: true),
            new Equipment(0, "Soldering Station", "EQ-004", isAvailable: true));

        await context.SaveChangesAsync();
    }
}