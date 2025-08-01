using MeetMe.Integration.Tests.Infrastructure;

namespace MeetMe.Integration.Tests.Collections;

/// <summary>
/// Test collection to ensure proper cleanup of shared resources
/// </summary>
[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<SharedTestDatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

/// <summary>
/// Fixture for managing the shared test database lifecycle across all tests
/// </summary>
public class SharedTestDatabaseFixture : IAsyncDisposable
{
    private SharedTestDatabase? _database;

    public async Task<SharedTestDatabase> GetDatabaseAsync()
    {
        _database ??= await SharedTestDatabase.GetInstanceAsync();
        return _database;
    }

    public async ValueTask DisposeAsync()
    {
        if (_database != null)
        {
            await _database.DisposeAsync();
        }
    }
}
