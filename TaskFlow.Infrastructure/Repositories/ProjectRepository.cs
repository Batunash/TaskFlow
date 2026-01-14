using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;
namespace TaskFlow.Infrastructure.Repositories
{
    public class ProjectRepository(AppDbContext db) : IProjectRepository
    {
       
        public async Task<bool> ExistsByNameAsync(string name, int organizationId, int? excludeProjectId = null)
        {
            var query = db.Projects.AsQueryable();
            query = query.Where(p => p.Name == name && p.OrganizationId == organizationId);
            if (excludeProjectId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProjectId.Value);
            }

            return await query.AnyAsync();
        }
        public async Task AddAsync(Project project)
        {
            await db.Projects.AddAsync(project);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Project project)
        {
            db.Projects.Remove(project);
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await db.Projects
               .Include(p => p.Tasks)
               .Include(p => p.ProjectMembers)
               .ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await db.Projects
               .Include(p => p.Tasks)
               .Include(p => p.ProjectMembers)
               .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<bool> IsMemberAsync(int projectId, int userId)
        {
            var project = await db.Projects
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
            {
                return false;
            }
            return project.ProjectMembers.Any(m => m.UserId == userId);
        }
        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }

    }
}
