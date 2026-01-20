using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories
{
    public class OrganizationRepository(AppDbContext db) : IOrganizationRepository
    {
        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await db.Organizations.AnyAsync(o => o.Name.ToLower() == name.ToLower());
        }
        public async Task AddAsync(Organization organization)
        {
            await db.Organizations.AddAsync(organization);
            await db.SaveChangesAsync();
        }

        public async Task<Organization?> GetByIdAsync(int id)
        {
            return await db.Organizations.FindAsync(id);
        }

        public async Task<Organization?> GetByUserIdAsync(int userId)
        {
           return await db.Organizations
                .FirstOrDefaultAsync(o => o.OwnerId == userId);
        }
        public async Task<Organization?> GetByIdWithMembersAsync(int id)
        {
            return await db.Organizations
                .Include(o => o.Members)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await db.SaveChangesAsync();
        }
    }
}
