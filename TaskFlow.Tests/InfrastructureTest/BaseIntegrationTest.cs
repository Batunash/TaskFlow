using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Npgsql;
using Respawn;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Tests.InfrastructureTest.Fixtures;
using Xunit;

namespace TaskFlow.Tests.InfrastructureTest
{
    [Collection("IntegrationTests")]
    public abstract class BaseIntegrationTest : IAsyncLifetime
    {
        protected readonly AppDbContext DbContext;
        protected readonly Mock<ICurrentTenantService> MockTenantService;
        protected readonly Mock<ICurrentUserService> MockUserService;

        private static Respawner? _respawner;
        private readonly string _connectionString;

        protected BaseIntegrationTest(SharedDatabaseFixture fixture)
        {
            _connectionString = fixture.ConnectionString;

            MockTenantService = new Mock<ICurrentTenantService>();
            MockUserService = new Mock<ICurrentUserService>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_connectionString)
                .ReplaceService<IModelCacheKeyFactory, NoModelCacheKeyFactory>()
                .Options;

            DbContext = new AppDbContext(
                options,
                MockTenantService.Object,
                MockUserService.Object
            );
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
            DbContext.Dispose();
            return Task.CompletedTask;
        }
    }
}
