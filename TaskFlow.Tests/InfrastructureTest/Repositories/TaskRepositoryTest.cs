using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Tests.InfrastructureTest.Fixtures;
using Xunit;

namespace TaskFlow.Tests.InfrastructureTest.Repositories
{
    public class TaskRepositoryTest : BaseIntegrationTest
    {
        private readonly TaskRepository _repository;

        public TaskRepositoryTest(SharedDatabaseFixture fixture) : base(fixture)
        {
            _repository = new TaskRepository(DbContext);
        }
        private async Task<(User user, Organization org, Project project, WorkflowState state)> SeedTaskEnvironmentAsync(string suffix = "")
        {
            var user = new User($"user{suffix}_{Guid.NewGuid()}", "hash", null);
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();

            var org = new Organization($"Org{suffix}_{Guid.NewGuid()}", user.Id);
            await DbContext.Organizations.AddAsync(org);
            await DbContext.SaveChangesAsync();

            MockTenantService.Setup(s => s.OrganizationId).Returns(org.Id);

            var project = new Project($"Proj{suffix}", "Desc", org.Id);
            project.AddMember(user.Id, Role.Admin);
            await DbContext.Projects.AddAsync(project);
            await DbContext.SaveChangesAsync();

            var workflow = new Workflow(project.Id);
            var state = new WorkflowState(workflow.Id, "To Do", isInitial: true);
            workflow.AddState(state); 

            await DbContext.Workflows.AddAsync(workflow);
            await DbContext.SaveChangesAsync();

            return (user, org, project, state);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Task_To_Database()
        {
            // Arrange
            var (user, org, project, state) = await SeedTaskEnvironmentAsync();
            var task = new TaskItem("New Task", project.Id, org.Id, state.Id);

            // Act
            await _repository.AddAsync(task);

            // Assert
            DbContext.ChangeTracker.Clear(); 
            var dbTask = await DbContext.TaskItems.FindAsync(task.Id);

            dbTask.Should().NotBeNull();
            dbTask!.Title.Should().Be("New Task");
            dbTask.ProjectId.Should().Be(project.Id);
            dbTask.WorkflowStateId.Should().Be(state.Id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Task_With_WorkflowState()
        {
            // Arrange
            var (user, org, project, state) = await SeedTaskEnvironmentAsync();
            var task = new TaskItem("Task With Include", project.Id, org.Id, state.Id);

            await DbContext.TaskItems.AddAsync(task);
            await DbContext.SaveChangesAsync();

            // Act
            DbContext.ChangeTracker.Clear();
            var result = await _repository.GetByIdAsync(task.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Task With Include");
            result.WorkflowState.Should().NotBeNull();
            result.WorkflowState!.Name.Should().Be("To Do");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_If_Task_Belongs_To_Another_Tenant()
        {
            // Arrange
            var (user1, org1, proj1, state1) = await SeedTaskEnvironmentAsync("1");
            var task1 = new TaskItem("Task Org 1", proj1.Id, org1.Id, state1.Id);
            await DbContext.TaskItems.AddAsync(task1);
            await DbContext.SaveChangesAsync();
            var (user2, org2, proj2, state2) = await SeedTaskEnvironmentAsync("2");
            MockTenantService.Setup(s => s.OrganizationId).Returns(org2.Id);

            // Act
            DbContext.ChangeTracker.Clear();
            var result = await _repository.GetByIdAsync(task1.Id); 
            // Assert
            result.Should().BeNull(); 
        }

        [Fact]
        public async Task GetByProjectIdAsync_Should_Return_Only_Project_Tasks()
        {
            // Arrange
            var (user, org, projectA, stateA) = await SeedTaskEnvironmentAsync();

            var projectB = new Project("Proj B", "Desc", org.Id);
            await DbContext.Projects.AddAsync(projectB);
            var workflowB = new Workflow(projectB.Id);
            var stateB = new WorkflowState(workflowB.Id, "Open");
            workflowB.AddState(stateB);
            await DbContext.Workflows.AddAsync(workflowB);
            await DbContext.SaveChangesAsync();
            var t1 = new TaskItem("Task A1", projectA.Id, org.Id, stateA.Id);
            var t2 = new TaskItem("Task A2", projectA.Id, org.Id, stateA.Id);
            var t3 = new TaskItem("Task B1", projectB.Id, org.Id, stateB.Id);

            await DbContext.TaskItems.AddRangeAsync(t1, t2, t3);
            await DbContext.SaveChangesAsync();

            // Act
            DbContext.ChangeTracker.Clear();
            var results = await _repository.GetByProjectIdAsync(projectA.Id);

            // Assert
            results.Should().HaveCount(2);
            results.Should().Contain(t => t.Title == "Task A1");
            results.Should().Contain(t => t.Title == "Task A2");
            results.Should().NotContain(t => t.Title == "Task B1");
        }

        [Fact]
        public async Task GetByFilterAsync_Should_Filter_By_AssignedUser_And_State()
        {
            // Arrange
            var (user, org, project, state1) = await SeedTaskEnvironmentAsync();
            var user2 = new User("user2", "hash", org.Id);
            await DbContext.Users.AddAsync(user2);

            var workflow = await DbContext.Workflows.FirstAsync(w => w.ProjectId == project.Id);
            var state2 = new WorkflowState(workflow.Id, "Done");
            workflow.AddState(state2);
            await DbContext.SaveChangesAsync();
            var t1 = new TaskItem("T1", project.Id, org.Id, state1.Id);
            t1.Assign(user.Id);
            var t2 = new TaskItem("T2", project.Id, org.Id, state2.Id);
            t2.Assign(user2.Id);
            var t3 = new TaskItem("T3", project.Id, org.Id, state1.Id);
            t3.Assign(user2.Id);
            await DbContext.TaskItems.AddRangeAsync(t1, t2, t3);
            await DbContext.SaveChangesAsync();
            var filterUser = new TaskFilterDto { AssignedUserId = user2.Id, projectId = project.Id };
            var resultUser = await _repository.GetByFilterAsync(filterUser);
            // Act 
            var filterState = new TaskFilterDto { WorkflowStateId = state1.Id, projectId = project.Id };
            var resultState = await _repository.GetByFilterAsync(filterState);

            // Assert
            resultUser.Items.Should().HaveCount(2); 
            resultState.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByFilterAsync_Should_Paginate_Correctly()
        {
            // Arrange
            var (user, org, project, state) = await SeedTaskEnvironmentAsync();

            var tasks = new List<TaskItem>();
            for (int i = 1; i <= 15; i++)
            {
                tasks.Add(new TaskItem($"Task {i}", project.Id, org.Id, state.Id));
            }
            await DbContext.TaskItems.AddRangeAsync(tasks);
            await DbContext.SaveChangesAsync();

            // Act - Page 1, Size 10
            var filterPage1 = new TaskFilterDto { pageNumber = 1, pageSize = 10, projectId = project.Id };
            var resultPage1 = await _repository.GetByFilterAsync(filterPage1);

            // Act - Page 2, Size 10
            var filterPage2 = new TaskFilterDto { pageNumber = 2, pageSize = 10, projectId = project.Id };
            var resultPage2 = await _repository.GetByFilterAsync(filterPage2);

            // Assert
            resultPage1.TotalCount.Should().Be(15);
            resultPage1.Items.Should().HaveCount(10);

            resultPage2.Items.Should().HaveCount(5);

        }

        [Fact]
        public async Task Update_Should_Persist_Changes()
        {
            // Arrange
            var (user, org, project, state) = await SeedTaskEnvironmentAsync();
            var task = new TaskItem("Old Title", project.Id, org.Id, state.Id);
            await _repository.AddAsync(task);

            // Act
            task.Update("Updated Title", "Updated Desc");
            await _repository.SaveChangesAsync();

            // Assert
            DbContext.ChangeTracker.Clear();
            var dbTask = await _repository.GetByIdAsync(task.Id);

            dbTask.Should().NotBeNull();
            dbTask!.Title.Should().Be("Updated Title");
            dbTask!.Description.Should().Be("Updated Desc");
            dbTask.LastModifiedAt.Should().NotBeNull(); 
        }

        [Fact]
        public async Task Delete_Should_SoftDelete_Task()
        {
            // Arrange
            var (user, org, project, state) = await SeedTaskEnvironmentAsync();
            var task = new TaskItem("To Delete", project.Id, org.Id, state.Id);
            await _repository.AddAsync(task);

            // Act 
            task.Delete();
            await _repository.SaveChangesAsync();

            // Assert
            DbContext.ChangeTracker.Clear();
            var repoResult = await _repository.GetByIdAsync(task.Id);
            repoResult.Should().BeNull();
            var dbResult = await DbContext.TaskItems.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == task.Id);
            dbResult.Should().NotBeNull();
            dbResult!.IsDeleted.Should().BeTrue();
            dbResult.DeletedAt.Should().NotBeNull();
        }
        [Fact]
        public async Task GetByFilterAsync_Should_Return_Empty_When_PageNumber_Is_Too_High()
        {
            // Arrange
            var (user, org, project, state) = await SeedTaskEnvironmentAsync();
            var tasks = new List<TaskItem>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(new TaskItem($"Task {i}", project.Id, org.Id, state.Id));
            }
            await DbContext.TaskItems.AddRangeAsync(tasks);
            await DbContext.SaveChangesAsync();

            // Act
            var filter = new TaskFilterDto { pageNumber = 2, pageSize = 10, projectId = project.Id };
            var result = await _repository.GetByFilterAsync(filter);

            // Assert
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(5); 
        }

        [Fact]
        public async Task GetByFilterAsync_Should_Filter_By_Both_AssignedUser_And_State()
        {
            // Arrange
            var (user, org, project, state1) = await SeedTaskEnvironmentAsync();
            var user2 = new User("User2", "hash", org.Id);
            await DbContext.Users.AddAsync(user2);
            var workflow = await DbContext.Workflows.FirstAsync(w => w.ProjectId == project.Id);
            var state2 = new WorkflowState(workflow.Id, "Done");
            workflow.AddState(state2);
            await DbContext.SaveChangesAsync();
            var t1 = new TaskItem("Target Task", project.Id, org.Id, state1.Id); t1.Assign(user.Id);
            var t2 = new TaskItem("Wrong State", project.Id, org.Id, state2.Id); t2.Assign(user.Id);
            var t3 = new TaskItem("Wrong User", project.Id, org.Id, state1.Id); t3.Assign(user2.Id);
            var t4 = new TaskItem("Wrong Both", project.Id, org.Id, state2.Id); t4.Assign(user2.Id);
            await DbContext.TaskItems.AddRangeAsync(t1, t2, t3, t4);
            await DbContext.SaveChangesAsync();

            // Act 
            var filter = new TaskFilterDto
            {
                projectId = project.Id,
                AssignedUserId = user.Id,
                WorkflowStateId = state1.Id
            };
            var result = await _repository.GetByFilterAsync(filter);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Title.Should().Be("Target Task");
        }
    }
}