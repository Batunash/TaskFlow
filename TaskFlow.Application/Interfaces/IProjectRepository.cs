using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces
{
    public interface IProjectRepository
    {
        Task<bool> ExistsByNameAsync(string name, int organizationId, int? excludeProjectId = null);
        Task AddAsync(Project project);
        Task<Project?> GetByIdAsync(int id);
        Task<IEnumerable<Project>> GetAllAsync();
        Task DeleteAsync(Project project);
        Task<bool> IsMemberAsync(int projectId, int userId);

        Task SaveChangesAsync();
    }
}
