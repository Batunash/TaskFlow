using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Tests.InfrastructureTest.Fixtures;
using Xunit;

namespace TaskFlow.Tests.InfrastructureTest.Repositories
{
    public class ProjectRepositoryTests : BaseIntegrationTest
    {
        private readonly ProjectRepository _repository;

        public ProjectRepositoryTests(SharedDatabaseFixture fixture) : base(fixture)
        {
            _repository = new ProjectRepository(DbContext);
        }
        private async Task<(User user, Organization org)> SeedOrganizationAsync(string suffix = "")
        {
            var user = new User($"user{suffix}_{Guid.NewGuid()}", "hash", null);
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();

            var org = new Organization($"Org{suffix}_{Guid.NewGuid()}", user.Id);
            await DbContext.Organizations.AddAsync(org);
            await DbContext.SaveChangesAsync();

            return (user, org);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Project_To_Database()
        {
            // Arrange
            var (user, org) = await SeedOrganizationAsync();
            MockTenantService.Setup(s => s.OrganizationId).Returns(org.Id); 

            var project = new Project("Test Project", "Description", org.Id);
            project.AddMember(user.Id, Role.Admin);

            // Act
            await _repository.AddAsync(project);
            DbContext.ChangeTracker.Clear(); 
            var dbProject = await DbContext.Projects.FindAsync(project.Id);

            dbProject.Should().NotBeNull();
            dbProject!.Name.Should().Be("Test Project");
            dbProject.OrganizationId.Should().Be(org.Id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Project_With_Includes()
        {
            // Arrange
            var (user, org) = await SeedOrganizationAsync();
            MockTenantService.Setup(s => s.OrganizationId).Returns(org.Id);

            var project = new Project("Rich Project", "Has Tasks and Members", org.Id);
            project.AddMember(user.Id, Role.Admin);
            await DbContext.Projects.AddAsync(project);
            await DbContext.SaveChangesAsync();

            // Act
            DbContext.ChangeTracker.Clear();
            var result = await _repository.GetByIdAsync(project.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Rich Project");
            result.ProjectMembers.Should().NotBeEmpty();
            result.ProjectMembers.First().UserId.Should().Be(user.Id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_Project_DoesNotExist()
        {
            // Arrange
            var randomId = 99999;

            // Act
            var result = await _repository.GetByIdAsync(randomId);

            // Assert
            result.Should().BeNull();
        }
        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_If_Project_Belongs_To_Another_Tenant()
        {
            // Arrange
            var (user1, org1) = await SeedOrganizationAsync("1");
            var project1 = new Project("Org1 Project", "Desc", org1.Id);
            await DbContext.Projects.AddAsync(project1);
            await DbContext.SaveChangesAsync();
            var (user2, org2) = await SeedOrganizationAsync("2");
            MockTenantService.Setup(s => s.OrganizationId).Returns(org2.Id);
            DbContext.ChangeTracker.Clear();
            var result = await _repository.GetByIdAsync(project1.Id);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Only_Current_Tenant_Projects()
        {
            // Arrange
            var (user1, org1) = await SeedOrganizationAsync("1");
            var (user2, org2) = await SeedOrganizationAsync("2");

            var p1 = new Project("P1", "Org1", org1.Id);
            var p2 = new Project("P2", "Org1", org1.Id);
            var p3 = new Project("P3", "Org2", org2.Id);

            await DbContext.Projects.AddRangeAsync(p1, p2, p3);
            await DbContext.SaveChangesAsync();
            MockTenantService.Setup(s => s.OrganizationId).Returns(org1.Id);

            // Act
            DbContext.ChangeTracker.Clear();
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2); 
            result.Should().Contain(p => p.Id == p1.Id);
            result.Should().Contain(p => p.Id == p2.Id);
            result.Should().NotContain(p => p.Id == p3.Id);
        }
        [Fact]
        public async Task ExistsByNameAsync_Should_Return_True_If_Exists_In_Same_Org()
        {
            // Arrange
            var (user, org) = await SeedOrganizationAsync();
            var project = new Project("Alpha", "Desc", org.Id);
            await DbContext.Projects.AddAsync(project);
            await DbContext.SaveChangesAsync();

            // Act
            var exists = await _repository.ExistsByNameAsync("Alpha", org.Id);

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_False_If_Name_Exists_In_Different_Org()
        {
            // Arrange
            var (u1, org1) = await SeedOrganizationAsync("1");
            var (u2, org2) = await SeedOrganizationAsync("2");

            var p1 = new Project("Alpha", "Desc", org1.Id);
            await DbContext.Projects.AddAsync(p1);
            await DbContext.SaveChangesAsync();
            var exists = await _repository.ExistsByNameAsync("Alpha", org2.Id);
            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_False_When_ExcludeProjectId_Matches()
        {
            var (user, org) = await SeedOrganizationAsync();
            var project = new Project("Alpha", "Desc", org.Id);
            await DbContext.Projects.AddAsync(project);
            await DbContext.SaveChangesAsync();
            var exists = await _repository.ExistsByNameAsync("Alpha", org.Id, excludeProjectId: project.Id);

            // Assert
            exists.Should().BeFalse(); 
        }
        [Fact]
        public async Task DeleteAsync_Should_Remove_Project()
        {
            // Arrange
            var (user, org) = await SeedOrganizationAsync();
            MockTenantService.Setup(s => s.OrganizationId).Returns(org.Id);

            var project = new Project("To Delete", "Desc", org.Id);
            await DbContext.Projects.AddAsync(project);
            await DbContext.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(project);

            // Assert
            DbContext.ChangeTracker.Clear();
            var dbProject = await DbContext.Projects.FindAsync(project.Id);
            dbProject.Should().BeNull();
        }

        [Fact]
        public async Task IsMemberAsync_Should_Return_True_If_User_Is_Member()
        {
            // Arrange
            var (user, org) = await SeedOrganizationAsync();
            var project = new Project("Team Project", "Desc", org.Id);

            project.AddMember(user.Id, Role.Member);

            await DbContext.Projects.AddAsync(project);
            await DbContext.SaveChangesAsync();

            // Act
            var isMember = await _repository.IsMemberAsync(project.Id, user.Id);

            // Assert
            isMember.Should().BeTrue();
        }

        [Fact]
        public async Task IsMemberAsync_Should_Return_False_If_User_Is_Not_Member()
        {
            // Arrange
            var (user, org) = await SeedOrganizationAsync();
            var project = new Project("Secret Project", "Desc", org.Id);
            await DbContext.Projects.AddAsync(project);
            await DbContext.SaveChangesAsync();

            // Act
            var isMember = await _repository.IsMemberAsync(project.Id, user.Id);

            // Assert
            isMember.Should().BeFalse();
        }
        [Fact]
        public async Task DeleteAsync_Should_SoftDelete_Project()
        {
            // Arrange
            var (user, org) = await SeedOrganizationAsync();
            MockTenantService.Setup(s => s.OrganizationId).Returns(org.Id);

            var project = new Project("Soft Delete Test", "Desc", org.Id);
            await DbContext.Projects.AddAsync(project);
            await DbContext.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(project);

            // Assert
            DbContext.ChangeTracker.Clear();

            var hiddenProject = await DbContext.Projects.FindAsync(project.Id);
            hiddenProject.Should().BeNull();
            var deletedProject = await DbContext.Projects
                .IgnoreQueryFilters() 
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            deletedProject.Should().NotBeNull();
            deletedProject!.IsDeleted.Should().BeTrue();
        }
        [Fact]
        public async Task Update_Should_Persist_Changes()
        {
            // Arrange
            var (user, org) = await SeedOrganizationAsync();
            MockTenantService.Setup(s => s.OrganizationId).Returns(org.Id);

            var project = new Project("Old Name", "Desc", org.Id);
            await _repository.AddAsync(project);

            // Act
            project.Update("New Name", "New Desc"); 
            await _repository.SaveChangesAsync(); 
            // Assert
            DbContext.ChangeTracker.Clear();
            var dbProject = await _repository.GetByIdAsync(project.Id);
            dbProject!.Name.Should().Be("New Name");
            dbProject.LastModifiedAt.Should().NotBeNull(); 
        }
    }
}