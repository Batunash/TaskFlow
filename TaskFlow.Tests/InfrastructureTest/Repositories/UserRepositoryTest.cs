using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Tests.InfrastructureTest.Fixtures;
using Xunit;

namespace TaskFlow.Tests.InfrastructureTest.Repositories
{
    public class UserRepositoryTest : BaseIntegrationTest
    {
        private readonly UserRepository _repository;

        public UserRepositoryTest(SharedDatabaseFixture fixture) : base(fixture)
        {
            _repository = new UserRepository(DbContext);
        }

        [Fact]
        public async Task AddAsync_Should_Persist_User_To_Database()
        {
            // Arrange
            var user = new User($"user_{Guid.NewGuid()}", "hashed_password");

            // Act
            await _repository.AddAsync(user);

            // Assert
            DbContext.ChangeTracker.Clear();
            var dbUser = await DbContext.Users.FindAsync(user.Id);

            dbUser.Should().NotBeNull();
            dbUser!.UserName.Should().Be(user.UserName);
            dbUser.PasswordHash.Should().Be("hashed_password");
            dbUser.OrganizationId.Should().BeNull(); 
        }

        [Fact]
        public async Task AddAsync_Should_Persist_User_With_OrganizationId()
        {
            // Arrange
            var owner = new User("OrgOwner", "hash");
            await DbContext.Users.AddAsync(owner);
            await DbContext.SaveChangesAsync();
            var org = new Organization("Test Org", owner.Id);
            await DbContext.Organizations.AddAsync(org);
            await DbContext.SaveChangesAsync();
            var user = new User("OrgUser", "hash", org.Id);

            // Act
            await _repository.AddAsync(user);

            // Assert
            DbContext.ChangeTracker.Clear();
            var dbUser = await _repository.GetByIdAsync(user.Id);
            dbUser.Should().NotBeNull();
            dbUser!.OrganizationId.Should().Be(org.Id);
        }

        [Fact]
        public async Task GetByUserNameAsync_Should_Return_Correct_User()
        {
            // Arrange
            var user1 = new User("AlphaUser", "hash1");
            var user2 = new User("BetaUser", "hash2");

            await _repository.AddAsync(user1);
            await _repository.AddAsync(user2);

            // Act
            DbContext.ChangeTracker.Clear();
            var result = await _repository.GetByUserNameAsync("AlphaUser");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(user1.Id);
            result.UserName.Should().Be("AlphaUser");
            result.PasswordHash.Should().Be("hash1");
        }

        [Fact]
        public async Task GetByUserNameAsync_Should_Return_Null_If_User_Does_Not_Exist()
        {
            // Act
            var result = await _repository.GetByUserNameAsync("NonExistentUser");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_If_Id_Does_Not_Exist()
        {
            // Act
            var result = await _repository.GetByIdAsync(99999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByUserNameAsync_Should_Be_Case_Sensitive_Depending_On_Db_Collation()
        {
            // Arrange
            var user = new User("mustafa", "hash");
            await _repository.AddAsync(user);
            // Act
            var resultLower = await _repository.GetByUserNameAsync("mustafa");
            var resultUpper = await _repository.GetByUserNameAsync("MUSTAFA");
            // Assert
            resultLower.Should().NotBeNull();
        }
        [Fact]
        public async Task AddAsync_Should_Throw_DbUpdateException_When_UserName_Already_Exists()
        {
            // Arrange
            var uniqueName = $"UniqueUser_{Guid.NewGuid()}";
            var user1 = new User(uniqueName, "hash1");
            await _repository.AddAsync(user1); 

            // Act
            var user2 = new User(uniqueName, "hash2"); 

            // Assert
            await _repository.Invoking(r => r.AddAsync(user2))
                .Should().ThrowAsync<DbUpdateException>()
                .WithMessage("*"); 
        }
    }
}