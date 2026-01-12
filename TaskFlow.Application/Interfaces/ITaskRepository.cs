using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Application.DTOs;
namespace TaskFlow.Application.Interfaces
{
    public interface ITaskRepository
    {
        Task<TaskItem?> GetByIdAsync(int taskId);
        Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(int projectId);
        Task AddAsync(TaskItem task);
        Task<PageResult<TaskItem>> GetByFilterAsync(TaskFilterDto filter);
        Task SaveChangesAsync();
    }
}
