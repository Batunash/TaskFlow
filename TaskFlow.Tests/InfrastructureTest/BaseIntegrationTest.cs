using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
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
        private readonly Func<Task> _resetDatabase;
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
                .UseSqlServer(_connectionString)
                .ReplaceService<IModelCacheKeyFactory, NoModelCacheKeyFactory>()
                .Options;
            DbContext = new AppDbContext(options, MockTenantService.Object, MockUserService.Object);
            DbContext.Database.EnsureCreated();
            _resetDatabase = async () =>
            {
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
            };
        }

        public async Task InitializeAsync()
        {
            await _resetDatabase();
        }
        public Task DisposeAsync()
        {
            DbContext.Dispose();
            return Task.CompletedTask;
        }
    }
}