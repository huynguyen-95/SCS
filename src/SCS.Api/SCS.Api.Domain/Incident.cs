using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SCS.Api.Domain;

public class Incident(int premiseId, string description, DateTimeOffset date, string filePath, string createdBy)
{
    public int Id { get; private set; }

    public int PremiseId { get; private set; } = premiseId;

    public string Description { get; private set; } = description;

    public DateTimeOffset Date { get; private set; } = date;

    public string FilePath { get; private set; } = filePath;

    public string CreatedBy { get; private set; } = createdBy;
}

public sealed class IncidentDomainConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id");

        builder.Property(i => i.PremiseId)
            .IsRequired()
            .HasColumnName("premise_id");

        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("description");

        builder.Property(i => i.Date)
            .IsRequired()
            .HasColumnName("date");

        builder.Property(i => i.FilePath)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("file_path");

        builder.Property(i => i.CreatedBy)
            .IsRequired()
            .HasColumnName("created_by");
    }
}
