using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.DTOs;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces
{
    public interface IWorkflowService
    {
        Task<WorkflowDto> CreateWorkflowAsync(int projectId);
        Task<WorkflowStateDto> AddStateAsync(int projectId,WorkflowStateDto stateDto);
        Task<WorkflowTransitionDto> AddTransitionAsync(int projectId,WorkflowTransitionDto transitionDto);
        Task<WorkflowDto> GetWorkflowAsync(int projectId);
        Task RemoveStateAsync(int projectId, int stateId);
        Task RemoveTransitionAsync(int projectId, int transitionId);
    }

}

