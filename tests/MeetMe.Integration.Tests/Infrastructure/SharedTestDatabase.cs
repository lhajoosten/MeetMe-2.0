using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MeetMe.Infrastructure.Data;
using Testcontainers.MsSql;
using DotNet.Testcontainers.Builders;

namespace MeetMe.Integration.Tests.Infrastructure;

/// <summary>
/// Shared test database that reuses the same Docker container across all tests
/// for much better performance and resource usage
/// </summary>
public sealed class SharedTestDatabase : IAsyncDisposable
{
    private static readonly SemaphoreSlim _initializationSemaphore = new(1, 1);
    private static SharedTestDatabase? _instance;
    private static bool _isInitialized;

    private readonly MsSqlContainer _container;
    private IServiceProvider? _serviceProvider;

    private SharedTestDatabase()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("TestPass123!")
            .WithCleanUp(true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .WithStartupCallback((container, ct) => 
            {
                Console.WriteLine($"SQL Server container started: {container.GetConnectionString()}");
                return Task.CompletedTask;
            })
            .Build();
    }

    public static async Task<SharedTestDatabase> GetInstanceAsync()
    {
        if (_instance != null && _isInitialized)
            return _instance;

        await _initializationSemaphore.WaitAsync();
        try
        {
            if (_instance == null || !_isInitialized)
            {
                _instance ??= new SharedTestDatabase();
                await _instance.InitializeAsync();
                _isInitialized = true;
            }
        }
        finally
        {
            _initializationSemaphore.Release();
        }

        return _instance;
    }

    private async Task InitializeAsync()
    {
        if (_isInitialized) return;

        Console.WriteLine("Starting shared SQL Server container...");
        await _container.StartAsync();
        Console.WriteLine($"SQL Server container ready: {_container.GetConnectionString()}");

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(_container.GetConnectionString())
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors());

        _serviceProvider = services.BuildServiceProvider();

        // Create database schema once
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();
        
        Console.WriteLine("Database schema created successfully");
    }

    public ApplicationDbContext CreateContext()
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("Database not initialized");

        var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public string GetConnectionString()
    {
        return _container.GetConnectionString();
    }

    /// <summary>
    /// Cleans all data from the database while preserving schema
    /// This is much faster than recreating containers
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        using var context = CreateContext();
        
        // Disable foreign key constraints temporarily for faster cleanup
        await context.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");
        
        // Clear all data in dependency order
        var tablesToClear = new[]
        {
            "SearchQueries",
            "Comments", 
            "Posts",
            "Attendances",
            "Meetings",
            "AspNetUserRoles",
            "AspNetUsers", 
            "Users",
            "AspNetRoles"
        };

        foreach (var table in tablesToClear)
        {
            try
            {
                // Safe to use string interpolation here as we control the table names (hardcoded whitelist)
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
                await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}]");
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
            }
            catch (Exception ex)
            {
                // Log but don't fail - some tables might not exist or be empty
                Console.WriteLine($"Warning: Could not clear table {table}: {ex.Message}");
            }
        }

        // Reset identity columns where applicable
        var identityTables = new[] { "SearchQueries" };
        foreach (var table in identityTables)
        {
            try
            {
                // Safe to use string interpolation here as we control the table names (hardcoded whitelist)
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
                await context.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ([{table}], RESEED, 0)");
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
            }
            catch
            {
                // Ignore - table might not have identity column
            }
        }

        // Re-enable foreign key constraints
        await context.Database.ExecuteSqlRawAsync("EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'");
    }

    public async ValueTask DisposeAsync()
    {
        if (_serviceProvider != null)
        {
            if (_serviceProvider is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();
        }

        if (_container != null)
        {
            Console.WriteLine("Disposing shared SQL Server container...");
            await _container.DisposeAsync();
        }

        _instance = null;
        _isInitialized = false;
    }
}
