using FluentValidation;
using FluentValidation.Results;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;
using Xunit;

namespace TaskFlow.Tests.ApplicationTest.Services
{
    public class WorkflowServiceTest
    {
        private readonly Mock<IWorkflowRepository> _mockWorkflowRepo;
        private readonly Mock<IProjectRepository> _mockProjectRepo; 
        private readonly Mock<IValidator<WorkflowStateDto>> _mockStateValidator;
        private readonly Mock<IValidator<WorkflowTransitionDto>> _mockTransitionValidator;
        private readonly WorkflowService _workflowService;

        public WorkflowServiceTest()
        {
            _mockWorkflowRepo = new Mock<IWorkflowRepository>();
            _mockProjectRepo = new Mock<IProjectRepository>();
            _mockStateValidator = new Mock<IValidator<WorkflowStateDto>>();
            _mockTransitionValidator = new Mock<IValidator<WorkflowTransitionDto>>();

            _workflowService = new WorkflowService(
                _mockWorkflowRepo.Object,
                _mockProjectRepo.Object, 
                _mockStateValidator.Object,
                _mockTransitionValidator.Object
            );
        }

        [Fact]
        public async Task CreateWorkflowAsync_Should_Create_And_Return_Dto()
        {
            // Arrange
            int projectId = 100;
            int organizationId = 1; 
            var project = new Project("Test Project", "Test Description", organizationId);
            typeof(Project).GetProperty("Id")?.SetValue(project, projectId);
            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId))
                .ReturnsAsync(project);
            // Act
            var result = await _workflowService.CreateWorkflowAsync(projectId);

            // Assert
            Assert.Equal(projectId, result.ProjectId);
            _mockWorkflowRepo.Verify(r => r.AddAsync(It.Is<Workflow>(w => w.ProjectId == projectId)), Times.Once);
        }

        [Fact]
        public async Task CreateWorkflowAsync_Should_Throw_NotFound_When_Project_Missing()
        {
            // Arrange
            int projectId = 999;
            _mockProjectRepo.Setup(r => r.GetByIdAsync(projectId))
                .ReturnsAsync((Project?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _workflowService.CreateWorkflowAsync(projectId));
        }

        [Fact]
        public async Task GetWorkflowAsync_Should_Return_Workflow_When_Exists()
        {
            // Arrange
            int projectId = 100;
            var workflow = new Workflow(projectId);

            var state = new WorkflowState(workflow.Id, "Open");
            typeof(WorkflowState).GetProperty("Id")?.SetValue(state, 1);
            workflow.AddState(state);
            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(projectId)).ReturnsAsync(workflow);

            // Act
            var result = await _workflowService.GetWorkflowAsync(projectId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(projectId, result.ProjectId);
            Assert.Single(result.States);
            Assert.Equal("Open", result.States[0].Name);
        }

        [Fact]
        public async Task GetWorkflowAsync_Should_Throw_NotFound_When_Workflow_Missing()
        {
            // Arrange
            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(It.IsAny<int>())).ReturnsAsync((Workflow?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _workflowService.GetWorkflowAsync(1));
        }

        [Fact]
        public async Task AddStateAsync_Should_Add_State_When_Valid()
        {
            // Arrange
            int projectId = 100;
            var dto = new WorkflowStateDto { Name = "In Progress", IsInitial = false };

            _mockStateValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ValidationResult());

            var workflow = new Workflow(projectId);
            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(projectId)).ReturnsAsync(workflow);

            // Act
            var result = await _workflowService.AddStateAsync(projectId, dto);

            // Assert
            Assert.Equal("In Progress", result.Name);
            Assert.Contains(workflow.States, s => s.Name == "In Progress");
            _mockWorkflowRepo.Verify(r => r.UpdateAsync(workflow), Times.Once);
        }

        [Fact]
        public async Task AddStateAsync_Should_Throw_NotFound_When_Workflow_Missing()
        {
            // Arrange
            var dto = new WorkflowStateDto { Name = "S" };
            _mockStateValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new ValidationResult());

            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(It.IsAny<int>())).ReturnsAsync((Workflow?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _workflowService.AddStateAsync(1, dto));
        }

        [Fact]
        public async Task AddTransitionAsync_Should_Add_Transition_When_Valid()
        {
            // Arrange
            int projectId = 100;
            int fromStateId = 1;
            int toStateId = 2;

            var dto = new WorkflowTransitionDto
            {
                FromStateId = fromStateId,
                ToStateId = toStateId,
                AllowedRoles = new List<string> { "Admin" }
            };

            _mockTransitionValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(new ValidationResult());

            var workflow = new Workflow(projectId);
            var state1 = new WorkflowState(workflow.Id, "Open");
            typeof(WorkflowState).GetProperty("Id")?.SetValue(state1, fromStateId);
            workflow.AddState(state1);
            
            var state2 = new WorkflowState(workflow.Id, "In Progress");
            typeof(WorkflowState).GetProperty("Id")?.SetValue(state2, toStateId);
            workflow.AddState(state2);

            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(projectId)).ReturnsAsync(workflow);

            // Act
            var result = await _workflowService.AddTransitionAsync(projectId, dto);

            // Assert
            Assert.Equal(fromStateId, result.FromStateId);
            Assert.Equal(toStateId, result.ToStateId);
            Assert.Contains(workflow.Transitions, t => t.FromStateId == fromStateId && t.ToStateId == toStateId);

            _mockWorkflowRepo.Verify(r => r.UpdateAsync(workflow), Times.Once);
        }

        [Fact]
        public async Task AddTransitionAsync_Should_Throw_NotFound_When_Workflow_Missing()
        {
            // Arrange
            var dto = new WorkflowTransitionDto { FromStateId = 1, ToStateId = 2 };
            _mockTransitionValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(new ValidationResult());

            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(It.IsAny<int>())).ReturnsAsync((Workflow?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _workflowService.AddTransitionAsync(1, dto));
        }

        [Fact]
        public async Task RemoveStateAsync_Should_Remove_State_When_Found()
        {
            // Arrange
            int projectId = 100;
            int stateId = 5;

            var workflow = new Workflow(projectId);
            var state = new WorkflowState(workflow.Id, "To Delete");
            typeof(WorkflowState).GetProperty("Id")?.SetValue(state, stateId);

            workflow.AddState(state);
            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(projectId)).ReturnsAsync(workflow);

            // Act
            await _workflowService.RemoveStateAsync(projectId, stateId);

            // Assert
            Assert.DoesNotContain(workflow.States, s => s.Id == stateId);
            _mockWorkflowRepo.Verify(r => r.UpdateAsync(workflow), Times.Once);
        }

        [Fact]
        public async Task RemoveStateAsync_Should_Throw_NotFound_When_State_Not_Found()
        {
            // Arrange
            int projectId = 100;
            int stateId = 99; 

            var workflow = new Workflow(projectId);
            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(projectId)).ReturnsAsync(workflow);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _workflowService.RemoveStateAsync(projectId, stateId));
        }

        [Fact]
        public async Task RemoveStateAsync_Should_Throw_NotFound_When_Workflow_Missing()
        {
            // Arrange
            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(It.IsAny<int>())).ReturnsAsync((Workflow?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _workflowService.RemoveStateAsync(1, 1));
        }

        [Fact]
        public async Task RemoveTransitionAsync_Should_Remove_Transition_When_Found()
        {
            // Arrange
            int projectId = 100;
            int transitionId = 10;
            int fromStateId = 1;
            int toStateId = 2;

            var workflow = new Workflow(projectId);
            var s1 = new WorkflowState(workflow.Id, "A"); typeof(WorkflowState).GetProperty("Id")?.SetValue(s1, fromStateId);
            var s2 = new WorkflowState(workflow.Id, "B"); typeof(WorkflowState).GetProperty("Id")?.SetValue(s2, toStateId);
            workflow.AddState(s1);
            workflow.AddState(s2);

            var transition = new WorkflowTransition(workflow.Id, fromStateId, toStateId, new List<string>());
            typeof(WorkflowTransition).GetProperty("Id")?.SetValue(transition, transitionId);

            workflow.AddTransition(transition);

            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(projectId)).ReturnsAsync(workflow);

            // Act
            await _workflowService.RemoveTransitionAsync(projectId, transitionId);

            // Assert
            Assert.DoesNotContain(workflow.Transitions, t => t.Id == transitionId);
            _mockWorkflowRepo.Verify(r => r.UpdateAsync(workflow), Times.Once);
        }

        [Fact]
        public async Task RemoveTransitionAsync_Should_Throw_NotFound_When_Transition_Not_Found()
        {
            // Arrange
            int projectId = 100;
            int transitionId = 99;

            var workflow = new Workflow(projectId);
            _mockWorkflowRepo.Setup(r => r.GetByProjectIdAsync(projectId)).ReturnsAsync(workflow);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _workflowService.RemoveTransitionAsync(projectId, transitionId));
        }
    }
}