using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SCS.Api.Domain;

public class Premise
{
    public int Id { get; private set; }

    public string Name { get; private set; }

    public Premise(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Premise name cannot be null or empty.", nameof(name));
        }

        Id = id;
        Name = name;
    }
}

public sealed class PremiseDomainConfiguration : IEntityTypeConfiguration<Premise>
{
    public void Configure(EntityTypeBuilder<Premise> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).IsRequired();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
    }
}