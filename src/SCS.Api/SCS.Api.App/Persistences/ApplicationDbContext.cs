using Microsoft.EntityFrameworkCore;

namespace SCS.Api.App.Persistences;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Domain.User> Users { get; set; }

    public DbSet<Domain.Premise> Premises { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Domain.User).Assembly);
    }
}