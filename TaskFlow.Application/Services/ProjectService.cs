using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Services
{
    public class ProjectService(IProjectRepository projectRepository,ICurrentTenantService currentTenantService,
        IValidator<CreateProjectDto> createProjectValidator,IValidator<UpdateProjectDto> updateProjectValidator,
        IValidator<AddProjectMemberDto> addMemberValidator,IValidator<RemoveProjectMemberDto> removeMemberValidator) : IProjectService
    {
        public async Task<ResponseProjectDto> CreateProjectAsync(CreateProjectDto request,int currentUserId)
        {
            await createProjectValidator.ValidateAndThrowAsync(request);
            var organizationId = GetRequiredOrganizationId();
            var exists = await projectRepository.ExistsByNameAsync(request.Name, organizationId);
            if (exists)
            {
                throw new BusinessRuleException($"Project with name '{request.Name}' already exists in this organization.");
            }
            var project = new Project(
                request.Name,
                request.Description,
                organizationId
            );
            project.AddMember(currentUserId, Role.Admin);
            await projectRepository.AddAsync(project);
            return new ResponseProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description
            };
        }


        public async Task<ResponseProjectDto> UpdateProjectAsync(UpdateProjectDto dto,int currentUserId)
        {
            await updateProjectValidator.ValidateAndThrowAsync(dto);
            var organizationId = GetRequiredOrganizationId();
            var exists = await projectRepository.ExistsByNameAsync(dto.Name, organizationId, dto.Id);
            if (exists)
            {
                throw new BusinessRuleException($"Project with name '{dto.Name}' already exists in this organization.");
            }
            var project = await projectRepository.GetByIdAsync(dto.Id);
            if (project == null)
            {
                throw new NotFoundException($"Project with ID {dto.Id} not found.");
            }
            EnsureSameTenant(project.OrganizationId, organizationId);

            if (!project.IsAdmin(currentUserId))
            {
                throw new UnauthorizedAccessException();
            }
            project.Update(dto.Name, dto.Description);

            await projectRepository.SaveChangesAsync();

            return new ResponseProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description
            };
        }

        public async Task DeleteProjectAsync(int projectId,int currentUserId)
        {
            var organizationId = GetRequiredOrganizationId();

            var project = await projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new NotFoundException($"Project with ID {projectId} not found.");
            }
            EnsureSameTenant(project.OrganizationId, organizationId);

            if (!project.IsAdmin(currentUserId))
            {
                throw new UnauthorizedAccessException();
            }
            await projectRepository.DeleteAsync(project);
        }
        public async Task<IEnumerable<ResponseProjectDto>> GetAllProjectsAsync(int currentUserId)
        {
            var organizationId = GetRequiredOrganizationId();

            var projects = await projectRepository.GetAllAsync();

            return projects
                .Where(p =>
                    p.OrganizationId == organizationId &&
                    p.IsMember(currentUserId))
                .Select(p => new ResponseProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                });
        }
        public async Task<ResponseProjectDto> GetProjectByIdAsync(int projectId,int currentUserId)
        {
            var organizationId = GetRequiredOrganizationId();

            var project = await projectRepository.GetByIdAsync(projectId)
                ?? throw new NotFoundException($"Project with ID {projectId} not found.");

            EnsureSameTenant(project.OrganizationId, organizationId);

            if (!project.IsMember(currentUserId))
            {
                throw new UnauthorizedAccessException();
            }

            return new ResponseProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description
            };

        }

        public async Task AddMemberAsync(AddProjectMemberDto dto,int currentUserId)
        {
            await addMemberValidator.ValidateAndThrowAsync(dto);
            var organizationId = GetRequiredOrganizationId();

            var project = await projectRepository.GetByIdAsync(dto.ProjectId);
            if (project == null)
            {
                throw new NotFoundException($"Project with ID {dto.ProjectId} not found.");
            }
            EnsureSameTenant(project.OrganizationId, organizationId);

            if (!project.IsAdmin(currentUserId))
            {
                throw new UnauthorizedAccessException();
            }
            project.AddMember(dto.UserId, dto.Role);

            await projectRepository.SaveChangesAsync();
        }
        public async Task RemoveMemberAsync(RemoveProjectMemberDto dto,int currentUserId)
        {
            await removeMemberValidator.ValidateAndThrowAsync(dto);
            var organizationId = GetRequiredOrganizationId();

            var project = await projectRepository.GetByIdAsync(dto.ProjectId);
            if (project == null)
            {
                throw new NotFoundException($"Project with ID {dto.ProjectId} not found.");
            }

            EnsureSameTenant(project.OrganizationId, organizationId);

            if (!project.IsAdmin(currentUserId))
            {
                throw new UnauthorizedAccessException();
            }
            project.RemoveMember(dto.UserId);

            await projectRepository.SaveChangesAsync();
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
