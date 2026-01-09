using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem?> GetByIdAsync(int taskId);
        Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(int projectId);
        Task AddAsync(TaskItem task);
        Task SaveChangesAsync();
    }
}
