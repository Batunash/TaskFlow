using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;
namespace TaskFlow.Infrastructure.Repositories
{
    public class WorkflowRepository(AppDbContext db) : IWorkflowRepository
    {
        public async Task AddAsync(Workflow workflow)
        {
            await db.Workflows.AddAsync(workflow);
            await db.SaveChangesAsync();
        }

        public async Task<Workflow?> GetByProjectIdAsync(int projectId)
        {
           return await db.Workflows
                .FirstOrDefaultAsync(w => w.ProjectId == projectId);
            
        }
        public async Task UpdateAsync(Workflow workflow)
        {
            db.Workflows.Update(workflow);
            await db.SaveChangesAsync();
        }

    }
}
