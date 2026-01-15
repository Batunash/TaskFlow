using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Tests.InfrastructureTest.Fixtures;
using Xunit;

namespace TaskFlow.Tests.InfrastructureTest.Repositories
{
    public class WorkflowRepositoryTests : BaseIntegrationTest
    {
        private readonly WorkflowRepository _repository;

        public WorkflowRepositoryTests(SharedDatabaseFixture fixture) : base(fixture)
        {
            _repository = new WorkflowRepository(DbContext);
        }
        private async Task<Project> SeedProjectAsync()
        {
            var user = new User($"WfUser_{Guid.NewGuid()}", "hash", null);
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();

            var org = new Organization($"WfOrg_{Guid.NewGuid()}", user.Id);
            await DbContext.Organizations.AddAsync(org);
            await DbContext.SaveChangesAsync();

            MockTenantService.Setup(s => s.OrganizationId).Returns(org.Id);

            var project = new Project($"WfProject_{Guid.NewGuid()}", "Desc", org.Id);
            await DbContext.Projects.AddAsync(project);
            await DbContext.SaveChangesAsync();

            return project;
        }

        [Fact]
        public async Task AddAsync_Should_Persist_Workflow_And_States()
        {
            // Arrange
            var project = await SeedProjectAsync();
            var workflow = new Workflow(project.Id);
            workflow.AddState(new WorkflowState(0, "To Do", isInitial: true));
            workflow.AddState(new WorkflowState(0, "Done", isFinal: true));

            // Act
            await _repository.AddAsync(workflow);

            // Assert
            DbContext.ChangeTracker.Clear();
            var dbWorkflow = await DbContext.Workflows
                .Include(w => w.States)
                .FirstOrDefaultAsync(w => w.Id == workflow.Id);

            dbWorkflow.Should().NotBeNull();
            dbWorkflow!.ProjectId.Should().Be(project.Id);
            dbWorkflow.States.Should().HaveCount(2);
            dbWorkflow.States.Should().Contain(s => s.Name == "To Do" && s.IsInitial);
            dbWorkflow.States.Should().Contain(s => s.Name == "Done" && s.IsFinal);
            dbWorkflow.States.All(s => s.Id > 0).Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Transitions_And_Roles()
        {
            // Arrange
            var project = await SeedProjectAsync();
            var workflow = new Workflow(project.Id);
            var state1 = new WorkflowState(0, "Open", isInitial: true);
            var state2 = new WorkflowState(0, "Closed", isFinal: true);
            workflow.AddState(state1);
            workflow.AddState(state2);
            await _repository.AddAsync(workflow);
            // Act 
            var allowedRoles = new List<string> { "Admin", "Manager" };
            var transition = new WorkflowTransition(workflow.Id, state1.Id, state2.Id, allowedRoles);
            workflow.AddTransition(transition);
            await _repository.UpdateAsync(workflow);
            // Assert
            DbContext.ChangeTracker.Clear();
            var result = await _repository.GetByProjectIdAsync(project.Id);
            result.Should().NotBeNull();
            result!.Transitions.Should().HaveCount(1);
            var dbTransition = result.Transitions.First();
            dbTransition.FromStateId.Should().Be(state1.Id);
            dbTransition.ToStateId.Should().Be(state2.Id);
            dbTransition.AllowedRoles.Should().Contain("Admin");
            dbTransition.AllowedRoles.Should().Contain("Manager");
            dbTransition.AllowedRoles.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByProjectIdAsync_Should_Return_Full_Workflow_Graph()
        {
            // Arrange
            var project = await SeedProjectAsync();
            var workflow = new Workflow(project.Id);
            var s1 = new WorkflowState(0, "A", isInitial: true);
            var s2 = new WorkflowState(0, "B");
            workflow.AddState(s1);
            workflow.AddState(s2);
            await _repository.AddAsync(workflow); 
            var t1 = new WorkflowTransition(workflow.Id, s1.Id, s2.Id, new[] { "User" });
            workflow.AddTransition(t1);
            await _repository.UpdateAsync(workflow);
            // Act
            DbContext.ChangeTracker.Clear();
            var result = await _repository.GetByProjectIdAsync(project.Id);
            // Assert
            result.Should().NotBeNull();
            result!.States.Should().HaveCount(2);
            result.Transitions.Should().HaveCount(1);
            result.ProjectId.Should().Be(project.Id);
        }

        [Fact]
        public async Task GetByProjectIdAsync_Should_Return_Null_If_NotFound()
        {
            // Act
            var result = await _repository.GetByProjectIdAsync(99999);

            // Assert
            result.Should().BeNull();
        }
        [Fact]
        public async Task UpdateAsync_Should_Throw_Concurrency_Exception_If_Row_Deleted()
        {
            // Arrange
            var project = await SeedProjectAsync();
            var workflow = new Workflow(project.Id);
            await _repository.AddAsync(workflow);
            DbContext.ChangeTracker.Clear();
            await DbContext.Database.ExecuteSqlRawAsync($"DELETE FROM \"Workflows\" WHERE \"Id\" = {workflow.Id}");
            // Act
            var detachedWorkflow = new Workflow(project.Id);
            typeof(Workflow).GetProperty("Id")!.SetValue(detachedWorkflow, workflow.Id);

            // Assert
            await _repository.Invoking(r => r.UpdateAsync(detachedWorkflow))
                .Should().ThrowAsync<DbUpdateConcurrencyException>();
        }
        [Fact]
        public async Task UpdateAsync_Should_Throw_Exception_When_Transition_Has_Invalid_StateId()
        {
            // Arrange
            var project = await SeedProjectAsync();
            var workflow = new Workflow(project.Id);
            var state1 = new WorkflowState(0, "Start", isInitial: true);
            var state2 = new WorkflowState(0, "End", isFinal: true);
            workflow.AddState(state1);
            workflow.AddState(state2);
            await _repository.AddAsync(workflow);

            // Act
            var invalidTransition = new WorkflowTransition(workflow.Id, state1.Id, 99999, new List<string>());

            // Assert
            Action act = () => workflow.AddTransition(invalidTransition);
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Transition states must exist in the workflow.");
        }
    }
}