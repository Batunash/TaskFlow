using Testcontainers.MsSql;
using Xunit;

namespace TaskFlow.Tests.InfrastructureTest.Fixtures
{
    public class SharedDatabaseFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _msSqlContainer;
        public string ConnectionString { get; private set; } = null!;

        public SharedDatabaseFixture()
        {
            _msSqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .Build();
        }
        public async Task InitializeAsync()
        {
            await _msSqlContainer.StartAsync();
            ConnectionString = _msSqlContainer.GetConnectionString();
        }

        public async Task DisposeAsync()
        {
            await _msSqlContainer.StopAsync();
        }
    }
    [CollectionDefinition("IntegrationTests")]
    public class DatabaseCollection : ICollectionFixture<SharedDatabaseFixture>
    {
    }
}