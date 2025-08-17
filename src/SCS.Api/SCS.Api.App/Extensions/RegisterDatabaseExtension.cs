using Microsoft.EntityFrameworkCore;
using SCS.Api.App.Persistences;

namespace SCS.Api.App.Extensions;

public static class RegisterDatabaseExtension
{
    public static void RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
    }
}
