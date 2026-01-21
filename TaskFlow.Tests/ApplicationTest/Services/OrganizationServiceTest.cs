using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;
using Xunit;

namespace TaskFlow.Tests.ApplicationTest.Services
{
    public class OrganizationServiceTest
    {
        private readonly Mock<IOrganizationRepository> _mockOrgRepo;
        private readonly Mock<ICurrentTenantService> _mockTenantService;
        private readonly Mock<ICurrentUserService> _mockUserService;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IValidator<CreateOrganizationDto>> _mockCreateValidator;
        private readonly Mock<IValidator<InviteUserDto>> _mockInviteValidator;
        private readonly OrganizationService _organizationService;

        public OrganizationServiceTest()
        {
            _mockOrgRepo = new Mock<IOrganizationRepository>();
            _mockTenantService = new Mock<ICurrentTenantService>();
            _mockUserService = new Mock<ICurrentUserService>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockCreateValidator = new Mock<IValidator<CreateOrganizationDto>>();
            _mockInviteValidator = new Mock<IValidator<InviteUserDto>>();

            _organizationService = new OrganizationService(
                _mockOrgRepo.Object,
                _mockTenantService.Object,
                _mockUserService.Object,
                _mockUserRepo.Object,
                _mockCreateValidator.Object,
                _mockInviteValidator.Object
            );
        }
        [Fact]
        public async Task CreateAsync_Should_Create_Organization_When_Valid()
        {
            // Arrange
            int userId = 1;
            var dto = new CreateOrganizationDto { Name = "Test Org" };
            _mockCreateValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new ValidationResult());
            _mockOrgRepo.Setup(r => r.ExistsByNameAsync(dto.Name)).ReturnsAsync(false);
            var user = new User("username", "hash", null); 
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _organizationService.CreateAsync(dto, userId);

            // Assert
            Assert.Equal(dto.Name, result.Name);
            _mockOrgRepo.Verify(r => r.AddAsync(It.IsAny<Organization>()), Times.Once);
            _mockOrgRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            Assert.Equal(0, user.OrganizationId); 
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_BusinessRuleException_When_Name_Exists()
        {
            // Arrange
            var dto = new CreateOrganizationDto { Name = "Existing Org" };

            _mockOrgRepo.Setup(r => r.ExistsByNameAsync(dto.Name)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _organizationService.CreateAsync(dto, 1));
        }
        [Fact]
        public async Task GetCurrentAsync_Should_Return_Organization_When_User_Is_Owner()
        {
            // Arrange
            int userId = 1;
            int orgId = 10;
            _mockUserService.Setup(s => s.UserId).Returns(userId);
            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);
            var organization = new Organization("My Org", userId); 
            typeof(Organization).GetProperty("Id")?.SetValue(organization, orgId);

            _mockOrgRepo.Setup(r => r.GetByIdWithMembersAsync(orgId)).ReturnsAsync(organization);

            // Act
            var result = await _organizationService.GetCurrentAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("My Org", result.Name);
        }

        [Fact]
        public async Task GetCurrentAsync_Should_Return_Organization_When_User_Is_Member()
        {
            // Arrange
            int ownerId = 99;
            int currentUserId = 1; 
            int orgId = 10;
            _mockUserService.Setup(s => s.UserId).Returns(currentUserId);
            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);

            var organization = new Organization("My Org", ownerId);
            typeof(Organization).GetProperty("Id")?.SetValue(organization, orgId);
            organization.AddMember(currentUserId, OrganizationRole.Member);

            _mockOrgRepo.Setup(r => r.GetByIdWithMembersAsync(orgId)).ReturnsAsync(organization);

            // Act
            var result = await _organizationService.GetCurrentAsync();

            // Assert
            Assert.Equal("My Org", result.Name);
        }

        [Fact]
        public async Task GetCurrentAsync_Should_Throw_Unauthorized_When_User_Is_Not_Member_Or_Owner()
        {
            // Arrange
            int ownerId = 99;
            int currentUserId = 1;
            int orgId = 10;

            _mockUserService.Setup(s => s.UserId).Returns(currentUserId);
            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);

            var organization = new Organization("My Org", ownerId);
            _mockOrgRepo.Setup(r => r.GetByIdWithMembersAsync(orgId)).ReturnsAsync(organization);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _organizationService.GetCurrentAsync());
        }

        [Fact]
        public async Task GetCurrentAsync_Should_Throw_NotFound_When_Organization_Does_Not_Exist()
        {
            // Arrange
            _mockUserService.Setup(s => s.UserId).Returns(1);
            _mockTenantService.Setup(s => s.OrganizationId).Returns(10);
            _mockOrgRepo.Setup(r => r.GetByIdWithMembersAsync(10)).ReturnsAsync((Organization?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _organizationService.GetCurrentAsync());
        }

        [Fact]
        public async Task GetCurrentAsync_Should_Return_Organization_When_User_Is_Accepted_Member()
        {
            // Arrange
            int ownerId = 99;
            int currentUserId = 1;
            int orgId = 10;
            _mockUserService.Setup(s => s.UserId).Returns(currentUserId);
            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);

            var organization = new Organization("My Org", ownerId);
            typeof(Organization).GetProperty("Id")?.SetValue(organization, orgId);
            organization.AddMember(currentUserId, OrganizationRole.Member);
            var member = organization.Members.First(m => m.UserId == currentUserId);
            member.AcceptInvitation(); 

            _mockOrgRepo.Setup(r => r.GetByIdWithMembersAsync(orgId)).ReturnsAsync(organization);
            // Act
            var result = await _organizationService.GetCurrentAsync();
            // Assert
            Assert.Equal("My Org", result.Name);
        }

        [Fact]
        public async Task GetCurrentAsync_Should_Throw_Unauthorized_When_User_Has_Not_Accepted_Invite()
        {
            // Arrange
            int ownerId = 99;
            int currentUserId = 1;
            int orgId = 10;
            _mockUserService.Setup(s => s.UserId).Returns(currentUserId);
            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);

            var organization = new Organization("My Org", ownerId);
            organization.AddMember(currentUserId, OrganizationRole.Member);
            _mockOrgRepo.Setup(r => r.GetByIdWithMembersAsync(orgId)).ReturnsAsync(organization);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _organizationService.GetCurrentAsync());
        }

        [Fact]
        public async Task InviteAsync_Should_FindUserByName_And_AddMember()
        {
            // Arrange
            int ownerId = 1;
            int orgId = 10;
            var dto = new InviteUserDto { UserName = "targetUser" }; 

            _mockInviteValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new ValidationResult());

            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);

            var organization = new Organization("My Org", ownerId);
            _mockOrgRepo.Setup(r => r.GetByIdAsync(orgId)).ReturnsAsync(organization);
            var targetUser = new User("targetUser", "hash", null);
            typeof(User).GetProperty("Id")?.SetValue(targetUser, 55);
            _mockUserRepo.Setup(r => r.GetByUserNameAsync("targetUser")).ReturnsAsync(targetUser);

            // Act
            await _organizationService.InviteAsync(dto, ownerId);

            // Assert
            Assert.Contains(organization.Members, m => m.UserId == 55 && m.Role == OrganizationRole.Member);
            Assert.Contains(organization.Members, m => m.UserId == 55 && m.IsAccepted == false);

            _mockOrgRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AcceptInvitationAsync_Should_Set_IsAccepted_True()
        {
            // Arrange
            int userId = 5;
            int orgId = 10;
            var organization = new Organization("Test Org", 99); 
            typeof(Organization).GetProperty("Id")?.SetValue(organization, orgId);

            organization.AddMember(userId, OrganizationRole.Member); 

            _mockOrgRepo.Setup(r => r.GetByIdWithMembersAsync(orgId)).ReturnsAsync(organization);
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User("user", "pw", null));

            // Act
            await _organizationService.AcceptInvitationAsync(orgId, userId);

            // Assert
            var member = organization.Members.First(m => m.UserId == userId);
            Assert.True(member.IsAccepted); 
            _mockOrgRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
