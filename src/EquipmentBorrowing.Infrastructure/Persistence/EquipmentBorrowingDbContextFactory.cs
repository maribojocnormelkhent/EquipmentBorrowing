using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EquipmentBorrowing.Infrastructure.Persistence;

public class EquipmentBorrowingDbContextFactory : IDesignTimeDbContextFactory<EquipmentBorrowingDbContext>
{
    public EquipmentBorrowingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__EquipmentBorrowing")
            ?? throw new InvalidOperationException(
                "Set the ConnectionStrings__EquipmentBorrowing environment variable before running dotnet ef.");

        var options = new DbContextOptionsBuilder<EquipmentBorrowingDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new EquipmentBorrowingDbContext(options);
    }
}