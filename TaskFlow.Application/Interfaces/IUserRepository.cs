using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces
{
    public interface IUserRepository
    {
        User? GetByUserName(string userName, int organizationId);
        void Add(User user);
    }
}
