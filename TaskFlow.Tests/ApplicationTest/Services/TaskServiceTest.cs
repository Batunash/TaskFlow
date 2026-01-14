using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Events;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;
using Xunit;
namespace TaskFlow.Tests.ApplicationTest.Services
{
    public class TaskServiceTest
    {
        private readonly Mock<ITaskRepository> _mockTaskRepo;
        private readonly Mock<IProjectRepository> _mockProjectRepo;
        private readonly Mock<ICurrentTenantService> _mockTenantService;
        private readonly Mock<IWorkflowRepository> _mockWorkflowRepo;
        private readonly Mock<IPublisher> _mockPublisher;
        private readonly Mock<IValidator<CreateTaskDto>> _mockCreateVal;
        private readonly Mock<IValidator<UpdateTaskDto>> _mockUpdateVal;
        private readonly Mock<IValidator<AssignTaskDto>> _mockAssignVal;
        private readonly Mock<IValidator<ChangeTaskStatusDto>> _mockStatusVal;
        private readonly Mock<IValidator<TaskFilterDto>> _mockFilterVal;
        private readonly TaskService _taskService;

        public TaskServiceTest()
        {
            _mockTaskRepo = new Mock<ITaskRepository>();
            _mockProjectRepo = new Mock<IProjectRepository>();
            _mockTenantService = new Mock<ICurrentTenantService>();
            _mockWorkflowRepo = new Mock<IWorkflowRepository>();
            _mockPublisher = new Mock<IPublisher>();
            _mockCreateVal = new Mock<IValidator<CreateTaskDto>>();
            _mockUpdateVal = new Mock<IValidator<UpdateTaskDto>>();
            _mockAssignVal = new Mock<IValidator<AssignTaskDto>>();
            _mockStatusVal = new Mock<IValidator<ChangeTaskStatusDto>>();
            _mockFilterVal = new Mock<IValidator<TaskFilterDto>>();
            _taskService = new TaskService(
                _mockTaskRepo.Object,
                _mockProjectRepo.Object,
                _mockTenantService.Object,
                _mockWorkflowRepo.Object,
                _mockPublisher.Object,
                _mockCreateVal.Object,
                _mockUpdateVal.Object,
                _mockAssignVal.Object,
                _mockStatusVal.Object,
                _mockFilterVal.Object
            );
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_BusinessRuleException_When_Workflow_Not_Defined()
        {
            // Arrange
            var dto = new CreateTaskDto { ProjectId = 1, Title = "Test" };
            _mockCreateVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);

            var project = new Project("P1", "D", 1);
            typeof(Project).GetProperty("Id")?.SetValue(project, 1);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);

            // Workflow YOK
            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(1)).ReturnsAsync((Workflow?)null);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _taskService.CreateAsync(dto, 1));
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_BusinessRuleException_When_InitialState_Not_Found()
        {
            // Arrange
            var dto = new CreateTaskDto { ProjectId = 1, Title = "Test" };
            _mockCreateVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);

            var project = new Project("P1", "D", 1);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);

            var workflow = new Workflow(1);
            var state = new WorkflowState(1, "State", isInitial: false); 
            workflow.AddState(state);

            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(1)).ReturnsAsync(workflow);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _taskService.CreateAsync(dto, 1));
        }
        [Fact]
        public async Task DeleteAsync_Should_SoftDelete_Task_When_User_Is_Admin()
        {
            // Arrange
            int taskId = 10;
            int currentUserId = 99;
            int organizationId = 1;

            _mockTenantService.Setup(s => s.OrganizationId).Returns(organizationId);

            var task = new TaskItem("Task 1", 1, organizationId, 1);
            typeof(TaskItem).GetProperty("Id")?.SetValue(task, taskId);
            _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);

            var project = new Project("P1", "D", organizationId);
            project.AddMember(currentUserId, TaskFlow.Domain.Enums.Role.Admin); 
            _mockProjectRepo.Setup(r => r.GetByIdAsync(task.ProjectId)).ReturnsAsync(project);

            // Act
            await _taskService.DeleteAsync(taskId, currentUserId);

            // Assert
            Assert.True(task.IsDeleted);
            _mockTaskRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_Unauthorized_When_User_Is_Not_Admin()
        {
            // Arrange
            int taskId = 10;
            int currentUserId = 99;
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);

            var task = new TaskItem("Task 1", 1, 1, 1);
            _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);

            var project = new Project("P1", "D", 1);
            project.AddMember(currentUserId, TaskFlow.Domain.Enums.Role.Member);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(task.ProjectId)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _taskService.DeleteAsync(taskId, currentUserId));
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_NotFoundException_When_Task_Does_Not_Exist()
        {
            // Arrange
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);
            _mockTaskRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((TaskItem?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _taskService.DeleteAsync(1, 1));
        }
        [Fact]
        public async Task UpdateAsync_Should_Update_Title_And_Description_When_User_Is_Member()
        {
            var dto = new UpdateTaskDto { Id = 10, Title = "New Title", Description = "New Desc" };
            int currentUserId = 5;
            _mockUpdateVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);

            var task = new TaskItem("Old Title", 1, 1, 1);
            typeof(TaskItem).GetProperty("Id")?.SetValue(task, dto.Id);
            _mockTaskRepo.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(task);

            var project = new Project("P1", "D", 1);
            project.AddMember(currentUserId, TaskFlow.Domain.Enums.Role.Member); 
            _mockProjectRepo.Setup(r => r.GetByIdAsync(task.ProjectId)).ReturnsAsync(project);
            // Act
            var result = await _taskService.UpdateAsync(dto, currentUserId);
            // Assert
            Assert.Equal("New Title", task.Title);
            Assert.Equal("New Desc", task.Description);
            _mockTaskRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_Unauthorized_When_User_Is_Not_Member()
        {
            // Arrange
            var dto = new UpdateTaskDto { Id = 10, Title = "T" };
            _mockUpdateVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);
            var task = new TaskItem("Old", 1, 1, 1);
            _mockTaskRepo.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(task);
            var project = new Project("P1", "D", 1);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(task.ProjectId)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _taskService.UpdateAsync(dto, 999));
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_NotFoundException_When_Task_Does_Not_Exist()
        {
            // Arrange
            var dto = new UpdateTaskDto { Id = 99 };
            _mockUpdateVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);
            _mockTaskRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TaskItem?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _taskService.UpdateAsync(dto, 1));
        }

        [Fact]
        public async Task AssignAsync_Should_Assign_User_When_Requestor_Is_Admin_And_Target_Is_Member()
        {
            // Arrange
            int taskId = 5;
            int adminId = 1;
            int targetUserId = 2;
            var dto = new AssignTaskDto { TaskId = taskId, UserId = targetUserId };
            _mockAssignVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);
            var task = new TaskItem("T", 1, 1, 1);
            _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);
            var project = new Project("P", "D", 1);
            project.AddMember(adminId, TaskFlow.Domain.Enums.Role.Admin); 
            project.AddMember(targetUserId, TaskFlow.Domain.Enums.Role.Member); 
            _mockProjectRepo.Setup(r => r.GetByIdAsync(task.ProjectId)).ReturnsAsync(project);

            // Act
            await _taskService.AssignAsync(dto, adminId);

            // Assert
            Assert.Equal(targetUserId, task.AssignedUserId);
            _mockTaskRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AssignAsync_Should_Throw_Unauthorized_When_Requestor_Is_Not_Admin()
        {
            // Arrange
            var dto = new AssignTaskDto { TaskId = 5, UserId = 2 };
            _mockAssignVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);

            var task = new TaskItem("T", 1, 1, 1);
            _mockTaskRepo.Setup(r => r.GetByIdAsync(dto.TaskId)).ReturnsAsync(task);

            var project = new Project("P", "D", 1);
            project.AddMember(1, TaskFlow.Domain.Enums.Role.Member); 
            _mockProjectRepo.Setup(r => r.GetByIdAsync(task.ProjectId)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _taskService.AssignAsync(dto, 1));
        }

        [Fact]
        public async Task AssignAsync_Should_Throw_BusinessRuleException_When_TargetUser_Is_Not_Project_Member()
        {
            // Arrange
            var dto = new AssignTaskDto { TaskId = 5, UserId = 99 }; 
            int adminId = 1;
            _mockAssignVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);

            var task = new TaskItem("T", 1, 1, 1);
            _mockTaskRepo.Setup(r => r.GetByIdAsync(dto.TaskId)).ReturnsAsync(task);

            var project = new Project("P", "D", 1);
            project.AddMember(adminId, TaskFlow.Domain.Enums.Role.Admin);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(task.ProjectId)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _taskService.AssignAsync(dto, adminId));
        }

        [Fact]
        public async Task ChangeStatusAsync_Should_Update_State_And_Publish_Event_When_Transition_Is_Valid()
        {
            // Arrange
            int taskId = 100;
            int adminId = 1;
            int fromState = 1;
            int toState = 2;
            var dto = new ChangeTaskStatusDto { TaskId = taskId, TargetStateId = toState };

            _mockStatusVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);
            var task = new TaskItem("Task", 1, 1, fromState);
            typeof(TaskItem).GetProperty("Id")?.SetValue(task, taskId);
            _mockTaskRepo.Setup(r => r.GetByIdAsync(taskId)).ReturnsAsync(task);
            var project = new Project("P", "D", 1);
            project.AddMember(adminId, TaskFlow.Domain.Enums.Role.Admin);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(task.ProjectId)).ReturnsAsync(project);
            var workflow = new Workflow(1);
            var s1 = new WorkflowState(1, "Open");
            typeof(WorkflowState).GetProperty("Id")?.SetValue(s1, fromState);
            var s2 = new WorkflowState(1, "Closed");
            typeof(WorkflowState).GetProperty("Id")?.SetValue(s2, toState);

            workflow.AddState(s1);
            workflow.AddState(s2);
            var transition = new WorkflowTransition(1, fromState, toState, new List<string>());
            workflow.AddTransition(transition);

            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(task.ProjectId)).ReturnsAsync(workflow);

            // Act
            await _taskService.ChangeStatusAsync(dto, adminId);

            // Assert
            Assert.Equal(toState, task.WorkflowStateId);
            _mockTaskRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockPublisher.Verify(p => p.Publish(It.IsAny<TaskStatusChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ChangeStatusAsync_Should_Throw_BusinessRuleException_When_Transition_Is_Invalid()
        {
            // Arrange
            var dto = new ChangeTaskStatusDto { TaskId = 1, TargetStateId = 2 };
            _mockStatusVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);

            var task = new TaskItem("T", 1, 1, 1); 
            _mockTaskRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

            var project = new Project("P", "D", 1);
            project.AddMember(1, TaskFlow.Domain.Enums.Role.Admin);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);

            var workflow = new Workflow(1);
            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(1)).ReturnsAsync(workflow);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => _taskService.ChangeStatusAsync(dto, 1));
        }

        [Fact]
        public async Task ChangeStatusAsync_Should_Throw_Unauthorized_When_User_Is_Not_Admin()
        {
            // Arrange
            var dto = new ChangeTaskStatusDto { TaskId = 1, TargetStateId = 2 };
            _mockStatusVal.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);

            var task = new TaskItem("T", 1, 1, 1);
            _mockTaskRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

            var project = new Project("P", "D", 1);
            project.AddMember(1, TaskFlow.Domain.Enums.Role.Member); 
            _mockProjectRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _taskService.ChangeStatusAsync(dto, 1));
        }

        [Fact]
        public async Task GetByProjectIdAsync_Should_Return_Tasks_When_User_Is_Member()
        {
            // Arrange
            int projectId = 10;
            int userId = 1;
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);

            var project = new Project("P", "D", 1);
            project.AddMember(userId, TaskFlow.Domain.Enums.Role.Member);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            var tasks = new List<TaskItem> { new TaskItem("T1", projectId, 1, 1) };
            _mockTaskRepo.Setup(r => r.GetByProjectIdAsync(projectId)).ReturnsAsync(tasks);

            // Act
            var result = await _taskService.GetByProjectIdAsync(projectId, userId);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal("T1", result[0].Title);
        }

        [Fact]
        public async Task GetByProjectIdAsync_Should_Throw_Unauthorized_When_User_Is_Not_Member()
        {
            // Arrange
            int projectId = 10;
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);

            var project = new Project("P", "D", 1);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _taskService.GetByProjectIdAsync(projectId, 1));
        }

        [Fact]
        public async Task GetByFilterAsync_Should_Throw_Unauthorized_When_Project_Provided_And_User_Not_Member()
        {
            // Arrange
            var filter = new TaskFilterDto { projectId = 10 };
            _mockFilterVal.Setup(v => v.ValidateAsync(filter, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());
            _mockTenantService.Setup(s => s.OrganizationId).Returns(1);
            var project = new Project("P", "D", 1);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(project);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _taskService.GetByFilterAsync(filter, 1));
        }
    }
}
