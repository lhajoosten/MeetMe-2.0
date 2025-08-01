using MeetMe.Integration.Tests.Infrastructure;
using MeetMe.Integration.Tests.Helpers;

namespace MeetMe.Integration.Tests.Base;

public abstract class IntegrationTestBase : IAsyncDisposable
{
    protected readonly SqlServerTestDatabase Database;
    protected readonly TestDatabaseHelper TestHelper;

    protected IntegrationTestBase()
    {
        Database = new SqlServerTestDatabase();
        TestHelper = new TestDatabaseHelper(Database);
    }

    protected async Task InitializeAsync()
    {
        await Database.InitializeAsync();
        await TestHelper.SeedTestDataAsync();
    }

    public async ValueTask DisposeAsync()
    {
        TestHelper.Dispose();
        await Database.DisposeAsync();
    }
}
