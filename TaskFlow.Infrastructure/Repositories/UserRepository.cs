using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public void Add(User user)
    {
        dbContext.Users.Add(user);
        dbContext.SaveChanges();
    }

    public User? GetByUserName(string userName, int organizationId)
    {
        return dbContext.Users
            .FirstOrDefault(u => u.UserName == userName && u.OrganizationId == organizationId);
    }
    public User? GetById(int id)
    {
        return dbContext.Users.Find(id);
    }
}


