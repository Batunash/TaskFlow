using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Respawn;
using TaskFlow.Infrastructure.Persistence;
using Testcontainers.MsSql;
using Xunit;

namespace TaskFlow.Tests.APITest;
[Collection("ApiTests")]
public abstract class BaseApiTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    public HttpClient Client { get; private set; }
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
[CollectionDefinition("ApiTests")]
public class ApiTestCollection : ICollectionFixture<ApiDatabaseFixture>{}
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public CustomWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(_connectionString));
        });
    }
}