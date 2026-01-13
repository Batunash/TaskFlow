using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces
{
    public interface IWorkflowRepository
    {
      
        Task<Workflow?> GetByProjectIdAsync(int projectId);
        Task AddAsync(Workflow workflow);
        Task UpdateAsync(Workflow workflow);
        
    }
}
