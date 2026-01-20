using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using System.Net.Http.Headers;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Identity;
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
        if (_respawner == null)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" },
                WithReseed = true
            });
        }

        await using var resetConn = new NpgsqlConnection(_connectionString);
        await resetConn.OpenAsync();
        await _respawner.ResetAsync(resetConn);
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    protected async Task AuthenticateAsync(
        string username = "testuser",
        string role = "User"
    )
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<JwtTokenGenerator>();

        var user = context.Users.FirstOrDefault(u => u.UserName == username);
        if (user == null)
        {
            user = new User(username, "HashedPassword");
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        var token = tokenGenerator.Generate(user.Id, 1, role);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
