using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.ToTable("Borrowings", t =>
        {
            t.HasCheckConstraint("CK_Borrowings_ExpectedAfterBorrowed",
                "\"ExpectedReturnDate\" > \"DateBorrowed\"");
            t.HasCheckConstraint("CK_Borrowings_ActualNotBeforeBorrowed",
                "\"ActualReturnDate\" IS NULL OR \"ActualReturnDate\" >= \"DateBorrowed\"");
            t.HasCheckConstraint("CK_Borrowings_StatusMatchesReturnDate",
                "(\"Status\" = 'Active' AND \"ActualReturnDate\" IS NULL) OR " +
                "(\"Status\" = 'Returned' AND \"ActualReturnDate\" IS NOT NULL)");
        });

        builder.HasKey(b => b.Id);

        builder.Property(b => b.DateBorrowed).IsRequired();
        builder.Property(b => b.ExpectedReturnDate).IsRequired();
        builder.Property(b => b.ActualReturnDate);

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Equipment>()
            .WithMany()
            .HasForeignKey(b => b.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // One active borrowing per item, enforced by PostgreSQL (BR-10).
        builder.HasIndex(b => b.EquipmentId)
            .IsUnique()
            .HasFilter("\"Status\" = 'Active'")
            .HasDatabaseName("UX_Borrowings_EquipmentId_Active");

        builder.HasIndex(b => new { b.StudentId, b.Status });
        builder.HasIndex(b => new { b.EquipmentId, b.DateBorrowed });
    }
}