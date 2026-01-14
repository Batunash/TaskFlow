using System;
using System.Collections.Generic;
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
    public class ProjectServiceTest
    {
        private readonly Mock<IProjectRepository> _mockProjectRepo;
        private readonly Mock<ICurrentTenantService> _mockTenantService;
        private readonly Mock<IValidator<CreateProjectDto>> _mockCreateVal;
        private readonly Mock<IValidator<UpdateProjectDto>> _mockUpdateVal;
        private readonly Mock<IValidator<AddProjectMemberDto>> _mockAddMemberVal;
        private readonly Mock<IValidator<RemoveProjectMemberDto>> _mockRemoveMemberVal;
        private readonly ProjectService _projectService;

        public ProjectServiceTest()
        {
            _mockProjectRepo = new Mock<IProjectRepository>();
            _mockTenantService = new Mock<ICurrentTenantService>();
            _mockCreateVal = new Mock<IValidator<CreateProjectDto>>();
            _mockUpdateVal = new Mock<IValidator<UpdateProjectDto>>();
            _mockAddMemberVal = new Mock<IValidator<AddProjectMemberDto>>();
            _mockRemoveMemberVal = new Mock<IValidator<RemoveProjectMemberDto>>();
            _projectService = new ProjectService(
                _mockProjectRepo.Object,
                _mockTenantService.Object,
                _mockCreateVal.Object,
                _mockUpdateVal.Object,
                _mockAddMemberVal.Object,
                _mockRemoveMemberVal.Object
            );
        }
        [Fact]
        public async Task CreateProjectAsync_Should_Create_Project_When_Valid()
        {
            // Arrange
            int userId = 1;
            int orgId = 10;
            var dto = new CreateProjectDto { Name = "New Project", Description = "Desc" };

            _mockCreateVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);
            _mockProjectRepo.Setup(r => r.ExistsByNameAsync(dto.Name, orgId, null)).ReturnsAsync(false);

            // Act
            var result = await _projectService.CreateProjectAsync(dto, userId);

            // Assert
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Description, result.Description);
            _mockProjectRepo.Verify(r => r.AddAsync(It.Is<Project>(p =>
                p.Name == dto.Name &&
                p.OrganizationId == orgId &&
                p.IsAdmin(userId) 
            )), Times.Once);
        }

        [Fact]
        public async Task CreateProjectAsync_Should_Throw_BusinessRuleException_When_Name_Exists()
        {
            // Arrange
            var dto = new CreateProjectDto { Name = "Existing Project" };
            int orgId = 10;

            _mockCreateVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);
            _mockProjectRepo.Setup(r => r.ExistsByNameAsync(dto.Name, orgId, null)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _projectService.CreateProjectAsync(dto, 1));
        }
        [Fact]
        public async Task UpdateProjectAsync_Should_Update_When_User_Is_Admin()
        {
            // Arrange
            int projectId = 100;
            int userId = 1;
            int orgId = 10;
            var dto = new UpdateProjectDto { Id = projectId, Name = "Updated Name", Description = "Updated Desc" };

            _mockUpdateVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);
            _mockProjectRepo.Setup(r => r.ExistsByNameAsync(dto.Name, orgId, projectId)).ReturnsAsync(false);

            var project = new Project("Old Name", "Old Desc", orgId);
            typeof(Project).GetProperty("Id")?.SetValue(project, projectId);
            project.AddMember(userId, Role.Admin); 
            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act
            var result = await _projectService.UpdateProjectAsync(dto, userId);

            // Assert
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("Updated Desc", result.Description);
            _mockProjectRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProjectAsync_Should_Throw_Unauthorized_When_User_Is_Not_Admin()
        {
            // Arrange
            var dto = new UpdateProjectDto { Id = 100, Name = "Up" };
            int userId = 2;
            int orgId = 10;
            _mockUpdateVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);

            var project = new Project("Name", "Desc", orgId);
            project.AddMember(userId, Role.Member); 

            _mockProjectRepo.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _projectService.UpdateProjectAsync(dto, userId));
        }

        [Fact]
        public async Task UpdateProjectAsync_Should_Throw_Unauthorized_When_Organization_Mismatch()
        {
            // Arrange
            var dto = new UpdateProjectDto { Id = 100 };
            int currentOrgId = 10;
            int otherOrgId = 20;

            _mockUpdateVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(currentOrgId);

            var project = new Project("Name", "Desc", otherOrgId);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _projectService.UpdateProjectAsync(dto, 1));
        }

        [Fact]
        public async Task DeleteProjectAsync_Should_Delete_When_User_Is_Admin()
        {
            // Arrange
            int projectId = 100;
            int userId = 1;
            int orgId = 10;

            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);

            var project = new Project("Name", "Desc", orgId);
            project.AddMember(userId, Role.Admin);

            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act
            await _projectService.DeleteProjectAsync(projectId, userId);

            // Assert
            _mockProjectRepo.Verify(r => r.DeleteAsync(project), Times.Once);
        }

        [Fact]
        public async Task DeleteProjectAsync_Should_Throw_NotFound_When_Project_Does_Not_Exist()
        {
            // Arrange
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Project?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _projectService.DeleteProjectAsync(1, 1));
        }

        [Fact]
        public async Task GetAllProjectsAsync_Should_Return_Only_Member_Projects_In_Organization()
        {
            // Arrange
            int userId = 1;
            int orgId = 10;
            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);

            var p1 = new Project("P1", "D", orgId);
            p1.AddMember(userId, Role.Member); 

            var p2 = new Project("P2", "D", orgId);
            var p3 = new Project("P3", "D", 99); 
            p3.AddMember(userId, Role.Member);

            var projects = new List<Project> { p1, p2, p3 };
            _mockProjectRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(projects);

            // Act
            var result = await _projectService.GetAllProjectsAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("P1", result.First().Name);
        }

        [Fact]
        public async Task GetProjectByIdAsync_Should_Return_Project_When_User_Is_Member()
        {
            // Arrange
            int projectId = 100;
            int userId = 1;
            int orgId = 10;
            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);

            var project = new Project("P", "D", orgId);
            project.AddMember(userId, Role.Viewer);

            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act
            var result = await _projectService.GetProjectByIdAsync(projectId, userId);

            // Assert
            Assert.Equal("P", result.Name);
        }

        [Fact]
        public async Task GetProjectByIdAsync_Should_Throw_Unauthorized_When_User_Is_Not_Member()
        {
            // Arrange
            int projectId = 100;
            _mockTenantService.Setup(s => s.OrganizationId).Returns(10);

            var project = new Project("P", "D", 10);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _projectService.GetProjectByIdAsync(projectId, 1));
        }
        [Fact]
        public async Task AddMemberAsync_Should_Add_Member_When_Requestor_Is_Admin()
        {
            // Arrange
            int projectId = 100;
            int adminId = 1;
            int newUserId = 2;
            int orgId = 10;
            var dto = new AddProjectMemberDto { ProjectId = projectId, UserId = newUserId, Role = Role.Member };

            _mockAddMemberVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);

            var project = new Project("P", "D", orgId);
            project.AddMember(adminId, Role.Admin);

            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act
            await _projectService.AddMemberAsync(dto, adminId);

            // Assert
            Assert.True(project.IsMember(newUserId));
            _mockProjectRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RemoveMemberAsync_Should_Remove_Member_When_Requestor_Is_Admin()
        {
            // Arrange
            int projectId = 100;
            int adminId = 1;
            int targetUserId = 2;
            int orgId = 10;
            var dto = new RemoveProjectMemberDto { ProjectId = projectId, UserId = targetUserId };

            _mockRemoveMemberVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(orgId);

            var project = new Project("P", "D", orgId);
            project.AddMember(adminId, Role.Admin);
            project.AddMember(targetUserId, Role.Member);

            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act
            await _projectService.RemoveMemberAsync(dto, adminId);

            // Assert
            Assert.False(project.IsMember(targetUserId));
            _mockProjectRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddMemberAsync_Should_Throw_Unauthorized_When_Requestor_Is_Not_Admin()
        {
            // Arrange
            var dto = new AddProjectMemberDto { ProjectId = 100, UserId = 2 };
            _mockAddMemberVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(10);
            var project = new Project("P", "D", 10);
            project.AddMember(1, Role.Member); 

            _mockProjectRepo.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _projectService.AddMemberAsync(dto, 1));
        }
    }
}
