using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Common;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Services
{
    public class TaskService(ITaskRepository taskRepository,IProjectRepository projectRepository,ICurrentTenantService currentTenantService) : ITaskService
    {
        public async Task<ResponseTaskDto> CreateAsync(CreateTaskDto request, int currentUserId)
        {
            var organizationId = GetRequiredOrganizationId();

            var project = await projectRepository.GetByIdAsync(request.ProjectId);
            if (project == null)
                throw new Exception("Project not found");

            EnsureSameTenant(project.OrganizationId, organizationId);

            if (!project.IsMember(currentUserId))
                throw new UnauthorizedAccessException();

            var task = project.CreateTask(request.Title);

            await taskRepository.AddAsync(task);

            return new ResponseTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status
            };
        }

        public async Task DeleteAsync(int taskId, int currentUserId)
        {
            var organizationId = GetRequiredOrganizationId();

            var task = await taskRepository.GetByIdAsync(taskId);
            if (task == null || task.IsDeleted)
                throw new Exception("Task not found");

            EnsureSameTenant(task.OrganizationId, organizationId);

            var project = await projectRepository.GetByIdAsync(task.ProjectId);
            if (!project.IsAdmin(currentUserId))
                throw new UnauthorizedAccessException();

            task.Delete();

            await taskRepository.SaveChangesAsync();
        }
        public async Task<IReadOnlyList<ResponseTaskDto>> GetByProjectIdAsync(int projectId, int currentUserId)
        {
            var organizationId = GetRequiredOrganizationId();

            var project = await projectRepository.GetByIdAsync(projectId);
            if (project == null)
                throw new Exception("Project not found");

            EnsureSameTenant(project.OrganizationId, organizationId);

            if (!project.IsMember(currentUserId))
                throw new UnauthorizedAccessException();

            var tasks = await taskRepository.GetByProjectIdAsync(projectId);

            return tasks
                .Where(t => !t.IsDeleted)
                .Select(t => new ResponseTaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status
                }).ToList();
        }
        public async Task<PageResult<ResponseTaskDto>> GetByFilterAsync(TaskFilterDto filter, int currentUserId)
        {
            var organizationId = GetRequiredOrganizationId();
            if (filter.projectId > 0)
            {
                var project = await projectRepository.GetByIdAsync(filter.projectId);
                if (project == null)
                    throw new Exception("Project not found");

                EnsureSameTenant(project.OrganizationId, organizationId);

                if (!project.IsMember(currentUserId))
                    throw new UnauthorizedAccessException("You are not a member of this project");
            }
            var pagedEntities = await taskRepository.GetByFilterAsync(filter);
            var dtos = pagedEntities.Items.Select(t => new ResponseTaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status
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
            var organizationId = GetRequiredOrganizationId();

            var task = await taskRepository.GetByIdAsync(dto.Id);
            if (task == null || task.IsDeleted)
                throw new Exception("Task not found");

            EnsureSameTenant(task.OrganizationId, organizationId);

            var project = await projectRepository.GetByIdAsync(task.ProjectId);
            if (!project.IsMember(currentUserId))
                throw new UnauthorizedAccessException();

            task.Update(dto.Title, dto.Description);

            await taskRepository.SaveChangesAsync();

            return new ResponseTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status
            };
        }
        public async  Task<ResponseTaskDto> AssignAsync(AssignTaskDto dto, int currentUserId)
        {
            var organizationId = GetRequiredOrganizationId();

            var task = await taskRepository.GetByIdAsync(dto.TaskId);
            if (task == null || task.IsDeleted)
                throw new Exception("Task not found");

            EnsureSameTenant(task.OrganizationId, organizationId);

            var project = await projectRepository.GetByIdAsync(task.ProjectId);
            if (!project.IsAdmin(currentUserId))
                throw new UnauthorizedAccessException();

            if (!project.IsMember(dto.UserId))
                throw new Exception("User is not project member");

            task.Assign(dto.UserId);

            await taskRepository.SaveChangesAsync();
            return new ResponseTaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Status = task.Status
            };

        }
        public async Task ChangeStatusAsync(ChangeTaskStatusDto dto,int currentUserId)
        {
            var task = await taskRepository.GetByIdAsync(dto.TaskId);
            if (task == null || task.IsDeleted)
                throw new Exception("Task not found");

            var project = await projectRepository.GetByIdAsync(task.ProjectId);
            if (!project.IsAdmin(currentUserId))
                throw new UnauthorizedAccessException();

            if (dto.Status == TaskItemStatus.InProgress)
                task.Start();
            else if (dto.Status == TaskItemStatus.Done)
                task.Complete();

            await taskRepository.SaveChangesAsync();
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
