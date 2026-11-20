using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students", t =>
        {
            t.HasCheckConstraint("CK_Students_StudentNumber_Normalized",
                "char_length(\"StudentNumber\") > 0 AND \"StudentNumber\" = btrim(\"StudentNumber\")");
            t.HasCheckConstraint("CK_Students_Name_NotBlank",
                "char_length(btrim(\"Name\")) > 0");
        });

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedOnAdd();

        builder.Property(s => s.StudentNumber)
            .IsRequired()
            .HasColumnType("citext")
            .HasMaxLength(50);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Contact)
            .HasMaxLength(150);

        builder.Property(s => s.IsAllowedToBorrow)
            .IsRequired();

        builder.HasIndex(s => s.StudentNumber)
            .IsUnique()
            .HasDatabaseName("UX_Students_StudentNumber");
    }
}