using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Tests.InfrastructureTest.Fixtures;
using Xunit;

namespace TaskFlow.Tests.InfrastructureTest.Repositories
{
    public class ActivityLogRepositoryTest : BaseIntegrationTest
    {
        private readonly ActivityLogRepository _repository;

        public ActivityLogRepositoryTest(SharedDatabaseFixture fixture) : base(fixture)
        {
            _repository = new ActivityLogRepository(DbContext);
        }
        private async Task<(User user, TaskItem task)> SeedLogEnvironmentAsync()
        {
            var user = new User($"LogUser_{Guid.NewGuid()}", "hash", null);
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();
            var org = new Organization($"LogOrg_{Guid.NewGuid()}", user.Id);
            await DbContext.Organizations.AddAsync(org);
            await DbContext.SaveChangesAsync();
            MockTenantService.Setup(s => s.OrganizationId).Returns(org.Id);
            var project = new Project("LogProject", "Desc", org.Id);
            await DbContext.Projects.AddAsync(project);
            await DbContext.SaveChangesAsync();
            var workflow = new Workflow(project.Id);
            var state = new WorkflowState(workflow.Id, "To Do", isInitial: true);
            workflow.AddState(state);
            await DbContext.Workflows.AddAsync(workflow);
            await DbContext.SaveChangesAsync();
            var task = new TaskItem("Task for Log", project.Id, org.Id, state.Id);
            await DbContext.TaskItems.AddAsync(task);
            await DbContext.SaveChangesAsync();

            return (user, task);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Log_To_Database()
        {
            // Arrange
            var (user, task) = await SeedLogEnvironmentAsync();

            var log = new ActivityLog(
                taskId: task.Id,
                oldState: "To Do",
                newState: "In Progress",
                userId: user.Id
            );

            // Act
            await _repository.AddAsync(log);

            // Assert
            DbContext.ChangeTracker.Clear(); 

            var dbLog = await DbContext.ActivityLogs.FirstOrDefaultAsync(l => l.Id == log.Id);

            dbLog.Should().NotBeNull();
            dbLog!.TaskId.Should().Be(task.Id);
            dbLog.UserId.Should().Be(user.Id);
            dbLog.OldState.Should().Be("To Do");
            dbLog.NewState.Should().Be("In Progress");
            dbLog.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task AddAsync_Should_Work_For_Multiple_Logs_On_Same_Task()
        {
            // Arrange
            var (user, task) = await SeedLogEnvironmentAsync();

            var log1 = new ActivityLog(task.Id, "To Do", "In Progress", user.Id);
            var log2 = new ActivityLog(task.Id, "In Progress", "Done", user.Id);

            // Act
            await _repository.AddAsync(log1);
            await _repository.AddAsync(log2);

            // Assert
            DbContext.ChangeTracker.Clear();
            var logs = await DbContext.ActivityLogs
                .Where(l => l.TaskId == task.Id)
                .OrderBy(l => l.Timestamp)
                .ToListAsync();

            logs.Should().HaveCount(2);
            logs[0].NewState.Should().Be("In Progress");
            logs[1].NewState.Should().Be("Done");
        }
    }
}