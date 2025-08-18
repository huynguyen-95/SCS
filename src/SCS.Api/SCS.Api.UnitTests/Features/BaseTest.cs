using Microsoft.EntityFrameworkCore;
using SCS.Api.App.Persistences;

namespace SCS.Api.UnitTests.Features;

public abstract class BaseTest : IDisposable
{
    protected ApplicationDbContext DbContext { get; }

    protected BaseTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new ApplicationDbContext(options);

        // Ensure the database is created
        DbContext.Database.EnsureCreated();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            DbContext?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
