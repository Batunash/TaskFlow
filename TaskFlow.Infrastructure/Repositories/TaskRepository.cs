using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskFlow.Application.Common;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Application.DTOs;
namespace TaskFlow.Infrastructure.Repositories
{
    public class TaskRepository(AppDbContext db) : ITaskRepository
    {
        public async Task AddAsync(TaskItem task)
        {
            await db.TaskItems.AddAsync(task);
            await db.SaveChangesAsync();
        }

        public async Task<TaskItem?> GetByIdAsync(int taskId)
        {
            return await db.TaskItems
                .Include(t => t.WorkflowState)
                .FirstOrDefaultAsync(t=> t.Id == taskId);
        }

        public async Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(int projectId)
        {
            return await db.TaskItems
                .Include(t => t.WorkflowState)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }
        public async Task<PageResult<TaskItem>> GetByFilterAsync(TaskFilterDto filter)
        {
            var query = db.TaskItems.AsQueryable();
            if (filter.projectId > 0)
            {
                query = query.Where(t => t.ProjectId == filter.projectId);
            }
            if (filter.WorkflowStateId.HasValue)
            {
                query = query.Where(t => t.WorkflowStateId == filter.WorkflowStateId.Value);
            }
            if (filter.AssignedUserId.HasValue)
            {
                query = query.Where(t => t.AssignedUserId == filter.AssignedUserId.Value);
            }
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.pageNumber - 1) * filter.pageSize) 
                .Take(filter.pageSize)                         
                .ToListAsync();

            return new PageResult<TaskItem>
            {
                Items = items,
                TotalCount = totalCount,
                PageCount = filter.pageNumber,
                PageSize = filter.pageSize
            };
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }

    }
}
