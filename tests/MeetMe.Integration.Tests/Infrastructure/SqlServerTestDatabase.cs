using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MeetMe.Infrastructure.Data;
using Testcontainers.MsSql;

namespace MeetMe.Integration.Tests.Infrastructure;

public class SqlServerTestDatabase : IAsyncDisposable
{
    private readonly MsSqlContainer _container;
    private ApplicationDbContext? _context;
    private IServiceScope? _scope;

    public SqlServerTestDatabase()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("TestPass123!")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(_container.GetConnectionString()));

        var serviceProvider = services.BuildServiceProvider();
        _scope = serviceProvider.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created and up to date
        await _context.Database.EnsureCreatedAsync();
    }

    public ApplicationDbContext GetContext()
    {
        if (_context == null)
            throw new InvalidOperationException("Database not initialized. Call InitializeAsync first.");
        
        return _context;
    }

    public string GetConnectionString()
    {
        return _container.GetConnectionString();
    }

    public async Task ResetAsync()
    {
        if (_context == null) return;
        
        // Clear all data but keep schema
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM SearchQueries");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Comments");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Posts");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Attendances");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Meetings");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM AspNetRoles");
        
        // Reset identity seeds only for tables that have identity columns
        // Note: Users table uses GUID primary keys, not identity
        var validIdentityTables = new[] { "SearchQueries", "AspNetRoles" }; // These use int identity
        
        foreach (var table in validIdentityTables)
        {
            try
            {
                // Safe to use string interpolation here as we control the table names (hardcoded whitelist)
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
                await _context.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ([{table}], RESEED, 0)");
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
            }
            catch
            {
                // Ignore errors for tables without identity columns
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
        }
        
        _scope?.Dispose();
        
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }
}
