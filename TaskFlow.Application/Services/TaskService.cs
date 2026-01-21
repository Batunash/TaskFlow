using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Events;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;
using FluentValidation;

namespace TaskFlow.Application.Services
{
    public class TaskService(ITaskRepository taskRepository,IProjectRepository projectRepository,
        ICurrentTenantService currentTenantService,IWorkflowRepository workflowRepository,
        IPublisher publisher, IValidator<CreateTaskDto> createTaskValidator,
        IValidator<UpdateTaskDto> updateTaskValidator,IValidator<AssignTaskDto> assignTaskValidator,
        IValidator<ChangeTaskStatusDto> changeStatusValidator,IValidator<TaskFilterDto> filterValidator) : ITaskService
    {
        public async Task<ResponseTaskDto> CreateAsync(CreateTaskDto request, int currentUserId)
        {
            await createTaskValidator.ValidateAndThrowAsync(request);
            var organizationId = GetRequiredOrganizationId();

            var project = await projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
            {
                throw new NotFoundException($"Project with ID {request.ProjectId} not found.");

            }
            var workflow = await workflowRepository.GetByProjectIdAsync(request.ProjectId);
            if (workflow == null)
            {
                throw new BusinessRuleException("Project workflow not defined.");
            }
            var initialState = workflow.States.FirstOrDefault(s => s.IsInitial);
            if (initialState == null)
            {
                throw new BusinessRuleException("Workflow has no initial state.");
            }
            EnsureSameTenant(project.OrganizationId, organizationId);

            if (!project.IsMember(currentUserId))
            {
                throw new UnauthorizedAccessException();
            }

            var task = project.CreateTask(request.Title, initialState.Id);

            await taskRepository.AddAsync(task);

            return new ResponseTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                StateName = initialState.Name,
                ProjectId = task.ProjectId
            };
        }

        public async Task DeleteAsync(int taskId, int currentUserId)
        {
            var organizationId = GetRequiredOrganizationId();

            var task = await taskRepository.GetByIdAsync(taskId);
            if (task == null || task.IsDeleted)
            {
                throw new NotFoundException($"Task with ID {taskId} not found.");
            }

            EnsureSameTenant(task.OrganizationId, organizationId);

            var project = await projectRepository.GetByIdAsync(task.ProjectId);
            if (project != null && !project.IsAdmin(currentUserId))
            {
                throw new UnauthorizedAccessException();
            }
            task.Delete();

            await taskRepository.SaveChangesAsync();
        }
        public async Task<IReadOnlyList<ResponseTaskDto>> GetByProjectIdAsync(int projectId, int currentUserId)
        {
            var organizationId = GetRequiredOrganizationId();

            var project = await projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new NotFoundException($"Project with ID {projectId} not found.");
            }
            EnsureSameTenant(project.OrganizationId, organizationId);

            if (!project.IsMember(currentUserId))
            {
                throw new UnauthorizedAccessException();
            }
            var tasks = await taskRepository.GetByProjectIdAsync(projectId);

            return tasks
                .Where(t => !t.IsDeleted)
                .Select(t => new ResponseTaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    ProjectId = t.ProjectId,
                    StateId = t.WorkflowStateId,
                    StateName = t.WorkflowState?.Name ?? string.Empty
                }).ToList();
        }
        public async Task<PageResult<ResponseTaskDto>> GetByFilterAsync(TaskFilterDto filter, int currentUserId)
        {
            await filterValidator.ValidateAndThrowAsync(filter);
            var organizationId = GetRequiredOrganizationId();
            if (filter.projectId > 0)
            {
                var project = await projectRepository.GetByIdAsync(filter.projectId);
                if (project == null)
                {
                    throw new NotFoundException($"Project with ID {filter.projectId} not found.");
                }
                EnsureSameTenant(project.OrganizationId, organizationId);

                if (!project.IsMember(currentUserId))
                {
                    throw new UnauthorizedAccessException("You are not a member of this project");
                }
            }
            var pagedEntities = await taskRepository.GetByFilterAsync(filter);
            var dtos = pagedEntities.Items.Select(t => new ResponseTaskDto
            {
                Id = t.Id,
                Title = t.Title,
                StateId = t.WorkflowStateId,
                StateName = t.WorkflowState?.Name ?? string.Empty,
                ProjectId = t.ProjectId
            }).ToList();
            return new PageResult<ResponseTaskDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                PageCount = pagedEntities.PageCount,
                PageSize = pagedEntities.PageSize
            };
        }

        public async Task<ResponseTaskDto> UpdateAsync(UpdateTaskDto dto, int currentUserId)
        {
            await updateTaskValidator.ValidateAndThrowAsync(dto);
            var organizationId = GetRequiredOrganizationId();

            var task = await taskRepository.GetByIdAsync(dto.Id);
            if (task == null || task.IsDeleted)
            {
                throw new NotFoundException($"Task with ID {dto.Id} not found.");
            }
            EnsureSameTenant(task.OrganizationId, organizationId);

            var project = await projectRepository.GetByIdAsync(task.ProjectId);
            if (project != null && !project.IsMember(currentUserId))
            {
                throw new UnauthorizedAccessException();
            }
            task.Update(dto.Title, dto.Description);

            await taskRepository.SaveChangesAsync();

            return new ResponseTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                StateId = task.WorkflowStateId,
                StateName = task.WorkflowState?.Name ?? string.Empty
            };
        }
        public async  Task<ResponseTaskDto> AssignAsync(AssignTaskDto dto, int currentUserId)
        {
            await assignTaskValidator.ValidateAndThrowAsync(dto);
            var organizationId = GetRequiredOrganizationId();

            var task = await taskRepository.GetByIdAsync(dto.TaskId);
            if (task == null || task.IsDeleted)
            {
                throw new NotFoundException($"Task with ID {dto.TaskId} not found.");
            }
            EnsureSameTenant(task.OrganizationId, organizationId);

            var project = await projectRepository.GetByIdAsync(task.ProjectId);
            if (project != null && !project.IsAdmin(currentUserId))
            {
                throw new UnauthorizedAccessException();
            }
            if (project != null && !project.IsMember(dto.UserId))
            {
                throw new BusinessRuleException("User is not a member of this project.");
            }
            task.Assign(dto.UserId);

            await taskRepository.SaveChangesAsync();
            return new ResponseTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                StateId = task.WorkflowStateId,
                StateName = task.WorkflowState?.Name ?? string.Empty
            };

        }
        public async Task ChangeStatusAsync(ChangeTaskStatusDto dto,int currentUserId)
        {
            await changeStatusValidator.ValidateAndThrowAsync(dto);
            var task = await taskRepository.GetByIdAsync(dto.TaskId);
            if (task == null || task.IsDeleted)
            {
                throw new NotFoundException($"Task with ID {dto.TaskId} not found.");
            }
            var oldStateName = task.WorkflowState?.Name ?? "Unknown";
            var project = await projectRepository.GetByIdAsync(task.ProjectId);
            if ((project != null && !project.IsAdmin(currentUserId)))
            {
                throw new UnauthorizedAccessException();
            }
            var workflow = await workflowRepository.GetByProjectIdAsync(task.ProjectId);
            if (workflow == null) 
            {
                throw new BusinessRuleException("Workflow not found for this project.");
            }
            var isValidTransition = workflow.Transitions.Any(t =>
                t.FromStateId == task.WorkflowStateId &&
                t.ToStateId == dto.TargetStateId);

            if (!isValidTransition)
            {
                throw new BusinessRuleException("Invalid state transition.");
            }
            var targetState = workflow.States.FirstOrDefault(s => s.Id == dto.TargetStateId);
            var newStateName = targetState?.Name ?? "Unknown";
            task.ChangeState(dto.TargetStateId);
            await taskRepository.SaveChangesAsync();
            await publisher.Publish(new TaskStatusChangedEvent(
                task.Id,
                oldStateName,
                newStateName,
                currentUserId
            ));
        }
        private int GetRequiredOrganizationId()
        {
            return currentTenantService.OrganizationId
                ?? throw new UnauthorizedAccessException("Organization context not found");
        }
        private void EnsureSameTenant(int entityOrganizationId, int currentOrganizationId)
        {
            if (entityOrganizationId != currentOrganizationId)
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to access this resource"
                );
            }
        }
    }
}
