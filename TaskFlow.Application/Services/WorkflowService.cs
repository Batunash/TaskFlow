using System;
using System.Collections.Generic;
using FluentValidation;
using TaskFlow.Application.DTOs;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Exceptions;
namespace TaskFlow.Application.Services
{
    public class WorkflowService(IWorkflowRepository workflowRepository,IProjectRepository projectRepository,
        IValidator<WorkflowStateDto> stateValidator,IValidator<WorkflowTransitionDto> transitionValidator) : IWorkflowService
    {
        public async Task<WorkflowDto> CreateWorkflowAsync(int projectId)
        {
            var project = await projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new NotFoundException($"Project with ID {projectId} not found.");
            }
            var workflow = new Workflow(projectId);
            await workflowRepository.AddAsync(workflow);
            return new WorkflowDto
            {
                Id = workflow.Id,
                ProjectId = workflow.ProjectId,
                States = new List<WorkflowStateDto>(),
                Transitions = new List<WorkflowTransitionDto>()
            };
        }

        public async Task<WorkflowDto> GetWorkflowAsync(int projectId)
        {
            var workflow= await workflowRepository
                .GetByProjectIdAsync(projectId)
                ?? throw new NotFoundException($"Workflow for project {projectId} not found.");
            return new WorkflowDto
            {
                Id = workflow.Id,
                ProjectId = workflow.ProjectId,
                States = workflow.States
                    .Select(state => new WorkflowStateDto
                    {
                        Id = state.Id,
                        Name = state.Name,
                        IsInitial = state.IsInitial,
                        IsFinal = state.IsFinal
                    })
                    .ToList(),
                Transitions = workflow.Transitions
                    .Select(t => new WorkflowTransitionDto
                    {
                        Id = t.Id,
                        FromStateId = t.FromStateId,
                        ToStateId = t.ToStateId,
                        AllowedRoles = t.AllowedRoles.ToList()
                    })
                    .ToList()
            };
        }
        public async Task<WorkflowStateDto> AddStateAsync(int projectId, WorkflowStateDto stateDto)
        {
            await stateValidator.ValidateAndThrowAsync(stateDto);
            var workflow = await workflowRepository
                .GetByProjectIdAsync(projectId)
                ?? throw new NotFoundException($"Workflow for project {projectId} not found.");
            var state = new WorkflowState(
                workflow.Id,
                stateDto.Name,
                stateDto.IsInitial,
                stateDto.IsFinal
            );

            workflow.AddState(state);
            await workflowRepository.UpdateAsync(workflow);

            return new WorkflowStateDto
            {
                Id = state.Id,
                Name = state.Name,
                IsInitial = state.IsInitial,
                IsFinal = state.IsFinal
            };
        }
        public async Task<WorkflowTransitionDto> AddTransitionAsync(int projectId, WorkflowTransitionDto transitionDto)
        {
            await transitionValidator.ValidateAndThrowAsync(transitionDto);
            var workflow = await workflowRepository
                .GetByProjectIdAsync(projectId)
                ?? throw new NotFoundException($"Workflow for project {projectId} not found.");

            var transition = new WorkflowTransition(
                workflow.Id,
                transitionDto.FromStateId,
                transitionDto.ToStateId,
                transitionDto.AllowedRoles.ToList()
            );
            workflow.AddTransition(transition);
            await workflowRepository.UpdateAsync(workflow);
            return new WorkflowTransitionDto
            {
                Id = transition.Id,
                FromStateId = transition.FromStateId,
                ToStateId = transition.ToStateId,
                AllowedRoles = transition.AllowedRoles.ToList()
            };
        }

        public async  Task RemoveStateAsync(int projectId, int stateId)
        {
            var workflow = await workflowRepository
                .GetByProjectIdAsync(projectId)
                ?? throw new NotFoundException($"Workflow for project {projectId} not found.");

            var state = workflow.States.FirstOrDefault(s => s.Id == stateId);
            if (state == null)
            {
                throw new NotFoundException($"State with ID {stateId} not found in project {projectId}.");
            }

            workflow.RemoveState(state);

            await workflowRepository.UpdateAsync(workflow);
        }

        public async Task RemoveTransitionAsync(int projectId, int transitionId)
        {
            var workflow = await workflowRepository
                .GetByProjectIdAsync(projectId)
                ?? throw new NotFoundException($"Workflow for project {projectId} not found.");
            var transition = workflow.Transitions
                .FirstOrDefault(t => t.Id == transitionId)
                ?? throw new NotFoundException($"Transition with ID {transitionId} not found.");
            workflow.RemoveTransition(transition);
            await workflowRepository.UpdateAsync(workflow);
        }
    }
}