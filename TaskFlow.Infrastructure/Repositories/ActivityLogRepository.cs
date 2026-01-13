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
    public class ActivityLogRepository(AppDbContext db) : IActivityLogRepository
    {
        public async Task AddAsync(ActivityLog log)
        {
            await db.ActivityLogs.AddAsync(log);
            await db.SaveChangesAsync();
        }
    }
}
