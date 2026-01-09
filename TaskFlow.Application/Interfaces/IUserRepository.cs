using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUserNameAsync(string userName, int organizationId);
        Task<User?> GetByIdAsync(int id);

        Task AddAsync(User user);
    }
}
