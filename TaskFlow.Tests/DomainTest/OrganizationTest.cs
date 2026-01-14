using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests.DomainTest
{
    public class OrganizationTest
    {
        [Fact]
        public void CreateOrganization_ShouldInitializeCorrectly_AndAddOwnerAsMember()
        {
            // Arrange
            var name = "Tech Corp";
            var ownerId = 100;

            // Act
            var organization = new Organization(name, ownerId);

            // Assert
            organization.Name.Should().Be(name);
            organization.OwnerId.Should().Be(ownerId);
            organization.Members.Should().HaveCount(1);
            organization.Members.First().UserId.Should().Be(ownerId);
            organization.Members.First().Role.Should().Be(OrganizationRole.Owner);
        }

        [Fact]
        public void CreateOrganization_ShouldThrowException_WhenNameIsEmpty()
        {
            // Arrange
            string emptyName = "";
            int ownerId = 1;

            // Act
            Action action = () => new Organization(emptyName, ownerId);

            // Assert
            action.Should().Throw<ArgumentException>()
                  .WithMessage("Name required");
        }

        [Fact]
        public void AddMember_ShouldAddNewMember_WhenUserIsNotAlreadyMember()
        {
            // Arrange
            var organization = new Organization("Test Org", ownerId: 1);
            var newUserId = 2;
            var role = OrganizationRole.Member;

            // Act
            organization.AddMember(newUserId, role);

            // Assert
            organization.Members.Should().HaveCount(2); 
            organization.Members.Should().Contain(m => m.UserId == newUserId && m.Role == role);
        }

        [Fact]
        public void AddMember_ShouldNotAddDuplicateMember_WhenUserIsAlreadyMember()
        {
            // Arrange
            var ownerId = 1;
            var organization = new Organization("Test Org", ownerId);
            organization.AddMember(2, OrganizationRole.Admin); 

            // Act
            organization.AddMember(2, OrganizationRole.Member); 

            // Assert
            organization.Members.Should().HaveCount(2); 
            var member = organization.Members.First(m => m.UserId == 2);
            member.Role.Should().Be(OrganizationRole.Admin);
        }

        [Fact]
        public void IsAdmin_ShouldReturnTrue_WhenUserIsAdmin()
        {
            // Arrange
            var organization = new Organization("Test Org", ownerId: 1);
            var adminUserId = 5;
            organization.AddMember(adminUserId, OrganizationRole.Admin);

            // Act
            var result = organization.IsAdmin(adminUserId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsAdmin_ShouldReturnFalse_WhenUserIsNotAdmin()
        {
            // Arrange
            var organization = new Organization("Test Org", ownerId: 1);
            var memberUserId = 6;
            organization.AddMember(memberUserId, OrganizationRole.Member);

            // Act
            var resultMember = organization.IsAdmin(memberUserId);
            var resultNonMember = organization.IsAdmin(999);

            // Assert
            resultMember.Should().BeFalse();
            resultNonMember.Should().BeFalse();
        }

        [Fact]
        public void IsOwner_ShouldReturnTrue_OnlyForOwnerId()
        {
            // Arrange
            var ownerId = 10;
            var otherUserId = 20;
            var organization = new Organization("My Company", ownerId);
            organization.AddMember(otherUserId, OrganizationRole.Admin);

            // Act
            var isOwnerResult = organization.IsOwner(ownerId);
            var isOtherResult = organization.IsOwner(otherUserId);

            // Assert
            isOwnerResult.Should().BeTrue();
            isOtherResult.Should().BeFalse();
        }
    }
}
