using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        public void Add(User user)
        {
            throw new NotImplementedException();
        }

        public User? GetByUserName(string userName, int organizationId)
        {
            throw new NotImplementedException();
        }
    }
}
