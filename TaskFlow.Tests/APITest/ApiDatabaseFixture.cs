using Testcontainers.MsSql;
namespace TaskFlow.Tests.APITest;

public class ApiDatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;
    public string ConnectionString { get; private set; } = null!;

    public ApiDatabaseFixture()
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
