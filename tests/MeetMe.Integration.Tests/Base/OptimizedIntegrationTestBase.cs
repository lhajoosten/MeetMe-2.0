using MeetMe.Integration.Tests.Infrastructure;
using MeetMe.Integration.Tests.Helpers;

namespace MeetMe.Integration.Tests.Base;

/// <summary>
/// Optimized integration test base that reuses containers and only cleans data between tests
/// </summary>
public abstract class OptimizedIntegrationTestBase : IAsyncDisposable
{
    protected readonly SharedTestDatabase Database;
    protected readonly TestDatabaseHelper TestHelper;

    protected OptimizedIntegrationTestBase()
    {
        // Get shared database instance - will reuse container across tests
        Database = SharedTestDatabase.GetInstanceAsync().GetAwaiter().GetResult();
        TestHelper = new TestDatabaseHelper(Database);
    }

    protected async Task InitializeAsync()
    {
        // Clean database instead of recreating container
        await Database.CleanDatabaseAsync();
        
        // Seed fresh test data
        await TestHelper.SeedTestDataAsync();
    }

    public virtual async ValueTask DisposeAsync()
    {
        // Don't dispose the shared database - it will be reused
        // Only dispose the test helper
        TestHelper.Dispose();
        
        // Clean up test data for next test
        try
        {
            await Database.CleanDatabaseAsync();
        }
        catch
        {
            // Ignore cleanup errors during disposal
        }
    }
}
