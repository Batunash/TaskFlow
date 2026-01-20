using Microsoft.EntityFrameworkCore;
using Moq;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
namespace TaskFlow.Tests.APITest;

public class ApiDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    public string ConnectionString { get; private set; } = null!;

    public ApiDatabaseFixture()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
         .WithImage("postgres:15-alpine")
         .WithDatabase("taskflow_test")
         .WithUsername("postgres")
         .WithPassword("postgres")
         .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        ConnectionString = _postgreSqlContainer.GetConnectionString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        using var context = new AppDbContext(
            options,
            Mock.Of<ICurrentTenantService>(),
            Mock.Of<ICurrentUserService>()
        );

        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
       await _postgreSqlContainer.StopAsync();
    }
}
