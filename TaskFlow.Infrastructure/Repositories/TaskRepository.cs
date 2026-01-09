using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;
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
                .FirstOrDefaultAsync(t=> t.Id == taskId);
        }

        public async Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(int projectId)
        {
            return await db.TaskItems
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();
        }
        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }

    }
}
