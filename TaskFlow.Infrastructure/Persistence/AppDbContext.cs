using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence
{
   public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Domain.Entities.User> Users => Set<Domain.Entities.User>();
        public DbSet<Domain.Entities.Organization> Organizations => Set<Domain.Entities.Organization>();
    }
}
