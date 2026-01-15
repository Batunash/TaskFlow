using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Tests.InfrastructureTest.Fixtures;
using Xunit;

namespace TaskFlow.Tests.InfrastructureTest.Repositories
{
    public class OrganizationRepositoryTests : BaseIntegrationTest
    {
        private readonly OrganizationRepository _repository;

        public OrganizationRepositoryTests(SharedDatabaseFixture fixture) : base(fixture)
        {
            _repository = new OrganizationRepository(DbContext);
        }
        private async Task<User> CreateUserAsync(string suffix = "")
        {
            var user = new User($"OrgUser{suffix}_{Guid.NewGuid()}", "hashedpass", null);
            await DbContext.Users.AddAsync(user);
            await DbContext.SaveChangesAsync();
            return user;
        }

        [Fact]
        public async Task AddAsync_Should_Add_Organization_And_Set_Owner_As_Member()
        {
            // Arrange
            var owner = await CreateUserAsync();
            var org = new Organization($"Test Corp {Guid.NewGuid()}", owner.Id);

            // Act
            await _repository.AddAsync(org);

            // Assert
            DbContext.ChangeTracker.Clear();
            var dbOrg = await DbContext.Organizations
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.Id == org.Id);
            dbOrg.Should().NotBeNull();
            dbOrg!.Name.Should().Be(org.Name);
            dbOrg.OwnerId.Should().Be(owner.Id);
            dbOrg.Members.Should().HaveCount(1);
            dbOrg.Members.First().UserId.Should().Be(owner.Id);
            dbOrg.Members.First().Role.Should().Be(OrganizationRole.Owner);
        }

        [Fact]
        public async Task GetByIdWithMembersAsync_Should_Include_Members_List()
        {
            // Arrange
            var owner = await CreateUserAsync("Owner");
            var memberUser = await CreateUserAsync("Member");
            var org = new Organization($"Team Org {Guid.NewGuid()}", owner.Id);
            org.AddMember(memberUser.Id, OrganizationRole.Admin);
            await _repository.AddAsync(org);

            // Act
            DbContext.ChangeTracker.Clear();
            var result = await _repository.GetByIdWithMembersAsync(org.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Members.Should().HaveCount(2); 
            var adminMember = result.Members.FirstOrDefault(m => m.UserId == memberUser.Id);
            adminMember.Should().NotBeNull();
            adminMember!.Role.Should().Be(OrganizationRole.Admin);
        }

        [Fact]
        public async Task GetByUserIdAsync_Should_Return_Organization_Owned_By_User()
        {
            // Arrange
            var user1 = await CreateUserAsync("1");
            var org1 = new Organization($"Org 1 {Guid.NewGuid()}", user1.Id);
            await _repository.AddAsync(org1);

            // Act
            DbContext.ChangeTracker.Clear();
            var result = await _repository.GetByUserIdAsync(user1.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(org1.Id);
        }

        [Fact]
        public async Task AddAsync_Should_Throw_DbUpdateException_When_Name_Is_Duplicate()
        {
            // Arrange
            var user1 = await CreateUserAsync("1");
            var user2 = await CreateUserAsync("2");
            var duplicateName = $"Unique Name {Guid.NewGuid()}"; 
            var org1 = new Organization(duplicateName, user1.Id);
            await _repository.AddAsync(org1); 

            // Act
            var org2 = new Organization(duplicateName, user2.Id); 
            // Assert
            await _repository.Invoking(r => r.AddAsync(org2))
                .Should().ThrowAsync<DbUpdateException>()
                .WithMessage("*"); 
        }

        [Fact]
        public void Constructor_Should_Throw_ArgumentException_When_Name_Is_Empty()
        {
            // Arrange
            var ownerId = 1;

            // Act & Assert
            Action act = () => new Organization("", ownerId);
            act.Should().Throw<ArgumentException>()
               .WithMessage("*Name required*");
            Action actWhiteSpace = () => new Organization("   ", ownerId);
            actWhiteSpace.Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task AddMember_Should_Not_Add_Duplicate_Member()
        {
            // Arrange
            var owner = await CreateUserAsync("Own");
            var member = await CreateUserAsync("Mem");
            var org = new Organization($"DupMember Org {Guid.NewGuid()}", owner.Id);
            await _repository.AddAsync(org);

            // Act
            var fetchedOrg = await _repository.GetByIdWithMembersAsync(org.Id);
            fetchedOrg!.AddMember(member.Id, OrganizationRole.Member); 
            fetchedOrg.AddMember(member.Id, OrganizationRole.Admin);  
            await _repository.SaveChangesAsync();

            // Assert
            DbContext.ChangeTracker.Clear();
            var result = await _repository.GetByIdWithMembersAsync(org.Id);
            result!.Members.Should().Contain(m => m.UserId == member.Id);
            result.Members.Count(m => m.UserId == member.Id).Should().Be(1);
        }

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_False_If_Organization_Does_Not_Exist()
        {
            // Act
            var result = await _repository.ExistsByNameAsync("Non Existent Org 12345");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByUserIdAsync_Should_Return_Null_If_User_Has_No_Organization()
        {
            // Arrange
            var user = await CreateUserAsync("NoOrg");

            // Act
            var result = await _repository.GetByUserIdAsync(user.Id);

            // Assert
            result.Should().BeNull();
        }
        [Fact]
        public async Task SaveChangesAsync_Should_Update_Audit_Fields()
        {
            // Arrange
            var owner = await CreateUserAsync();
            var org = new Organization($"Audit Org {Guid.NewGuid()}", owner.Id);
            await _repository.AddAsync(org);
            var createdAt = org.CreatedAt;
            await Task.Delay(100);

            // Act
            var fetchedOrg = await _repository.GetByIdAsync(org.Id);
            fetchedOrg!.Update($"Updated Name {Guid.NewGuid()}");

            await _repository.SaveChangesAsync();

            // Assert
            DbContext.ChangeTracker.Clear();
            var updatedOrg = await _repository.GetByIdAsync(org.Id);
            updatedOrg!.LastModifiedAt.Should().NotBeNull();
            updatedOrg!.LastModifiedAt.Should().BeAfter(createdAt);
        }

    }
}