using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces
{
    public interface IOrganizationRepository
    {
        Task<bool> ExistsByNameAsync(string name);
        Task AddAsync(Organization organization);
        Task<Organization?> GetByIdAsync(int id);
        Task<Organization?> GetByUserIdAsync(int userId);
        Task<Organization?> GetByIdWithMembersAsync(int id);
        Task<List<Organization>> GetPendingInvitationsByUserIdAsync(int userId);
        Task SaveChangesAsync();
    }
}
