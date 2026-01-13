using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces
{
    public interface IActivityLogRepository
    {
        Task AddAsync(ActivityLog log);
    }
}
