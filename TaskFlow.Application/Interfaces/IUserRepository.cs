using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces
{
    public interface IUserRepository
    {
        User? GetByUserName(string userName, int organizationId);
        User? GetById(int id);
        void Add(User user);
    }
}
