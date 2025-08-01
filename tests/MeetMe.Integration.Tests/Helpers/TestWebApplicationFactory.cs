using MeetMe.API;
using MeetMe.Infrastructure.Data;
using MeetMe.Integration.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeetMe.Integration.Tests.Helpers;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private static SqlServerTestDatabase? _testDatabase;
    private static readonly object _lock = new object();

    public static SqlServerTestDatabase GetTestDatabase()
    {
        if (_testDatabase == null)
        {
            lock (_lock)
            {
                if (_testDatabase == null)
                {
                    _testDatabase = new SqlServerTestDatabase();
                    _testDatabase.InitializeAsync().Wait();
                }
            }
        }
        return _testDatabase;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Get the test database connection string synchronously
            var testDb = GetTestDatabase();
            var connectionString = testDb.GetConnectionString();

            // Add SQL Server test database
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Don't dispose the static test database here, it will be cleaned up at the end of all tests
        }
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        // Don't dispose the static test database here, it will be cleaned up at the end of all tests
        await base.DisposeAsync();
    }
}
