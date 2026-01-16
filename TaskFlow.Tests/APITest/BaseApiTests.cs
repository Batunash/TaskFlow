using Microsoft.Extensions.DependencyInjection;
using Respawn;
using TaskFlow.Infrastructure.Persistence;
using Xunit;

namespace TaskFlow.Tests.APITest;

public abstract class BaseApiTests
    : IAsyncLifetime, IClassFixture<ApiDatabaseFixture>
{
    protected readonly CustomWebApplicationFactory _factory;
    protected readonly HttpClient Client;

    private static Respawner? _respawner;
    private readonly string _connectionString;

    protected BaseApiTests(ApiDatabaseFixture fixture)
    {
        _connectionString = fixture.ConnectionString;
        _factory = new CustomWebApplicationFactory(_connectionString);
        Client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.EnsureCreatedAsync();
        }

        if (_respawner == null)
        {
            using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            await conn.OpenAsync();

            _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                SchemasToInclude = new[] { "dbo" },
                WithReseed = true
            });
        }

        using var resetConn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
        await resetConn.OpenAsync();
        await _respawner.ResetAsync(resetConn);
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }
}
