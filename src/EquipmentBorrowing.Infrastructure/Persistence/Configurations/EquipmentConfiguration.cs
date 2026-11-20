using EquipmentBorrowing.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment", t =>
        {
            t.HasCheckConstraint("CK_Equipment_Identifier_Normalized",
                "char_length(\"Identifier\") > 0 AND \"Identifier\" = btrim(\"Identifier\")");
            t.HasCheckConstraint("CK_Equipment_Name_NotBlank",
                "char_length(btrim(\"Name\")) > 0");
        });

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.Identifier)
            .IsRequired()
            .HasColumnType("citext")
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.IsAvailable)
            .IsRequired();

        builder.HasIndex(e => e.Identifier)
            .IsUnique()
            .HasDatabaseName("UX_Equipment_Identifier");
    }
}