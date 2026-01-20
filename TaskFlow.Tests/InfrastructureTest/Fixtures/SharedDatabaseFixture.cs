using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Persistence;
using Xunit;

namespace TaskFlow.Tests.InfrastructureTest.Fixtures
{
    public class SharedDatabaseFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer;
        public string ConnectionString { get; private set; } = null!;

        public SharedDatabaseFixture()
        {
            _postgresContainer = new PostgreSqlBuilder("postgres:15-alpine")
                .WithDatabase("taskflow_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();
            ConnectionString = _postgresContainer.GetConnectionString();

            // 🔥 MIGRATION SADECE 1 KEZ
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
            await _postgresContainer.StopAsync();
        }
    }

    [CollectionDefinition("IntegrationTests")]
    public class DatabaseCollection : ICollectionFixture<SharedDatabaseFixture>
    {
    }
}
