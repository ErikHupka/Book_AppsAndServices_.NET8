using Microsoft.Data.SqlClient; // SqlConnectionStringBuilder
using Microsoft.EntityFrameworkCore; // UseSqlServer
using Microsoft.Extensions.DependencyInjection; // IServiceCollection
namespace Northwind.EntityModels;
public static class NorthwindContextExtensions
{
    /// <summary>
    /// Adds NorthwindContext to the specified IServiceCollection. Uses the SqlServer database provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">Set to override the default.</param>
    /// <returns>An IServiceCollection that can be used to add more services.</returns>
    public static IServiceCollection AddNorthwindContext(
      this IServiceCollection services,
      string? connectionString = null)
    {
        if (connectionString == null)
        {
            SqlConnectionStringBuilder builder = new()
            {
                DataSource = "tcp:apps-services-book-dvlpr.database.windows.net,1433",
                InitialCatalog = "Northwind",
                TrustServerCertificate = true,
                MultipleActiveResultSets = true,
                ConnectTimeout = 3,
                UserID = Environment.GetEnvironmentVariable("MY_SQL_USR"),
                Password = Environment.GetEnvironmentVariable("MY_SQL_PWD")
            };
            // If using SQL Server authentication.
            // builder.UserID = Environment.GetEnvironmentVariable("MY_SQL_USR");
            // builder.Password = Environment.GetEnvironmentVariable("MY_SQL_PWD");
            connectionString = builder.ConnectionString;
        }
        services.AddDbContext<NorthwindContext>(options =>
        {
            options.UseSqlServer(connectionString);
            // Log to console when executing EF Core commands.
            options.LogTo(Console.WriteLine,
              new[] { Microsoft.EntityFrameworkCore
          .Diagnostics.RelationalEventId.CommandExecuting });
        },
        // Register with a transient lifetime to avoid concurrency 
        // issues with Blazor Server projects.
        contextLifetime: ServiceLifetime.Transient,
        optionsLifetime: ServiceLifetime.Transient);
        return services;
    }
}