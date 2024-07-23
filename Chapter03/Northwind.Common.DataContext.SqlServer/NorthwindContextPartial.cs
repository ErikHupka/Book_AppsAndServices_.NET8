using Microsoft.Data.SqlClient; // SqlConnectionStringBuilder
using Microsoft.EntityFrameworkCore; // DbContext
namespace Northwind.EntityModels;
public partial class NorthwindContext : DbContext
{
    private static readonly SetLastRefreshedInterceptor
      setLastRefreshedInterceptor = new();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
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
        }
        optionsBuilder.AddInterceptors(setLastRefreshedInterceptor);
    }
}