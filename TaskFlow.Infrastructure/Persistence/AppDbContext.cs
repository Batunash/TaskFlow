using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Identity;

namespace TaskFlow.Infrastructure.Persistence
{
   public class AppDbContext(DbContextOptions<AppDbContext> options,CurrentTenantService currentTenantService) : DbContext(options)
    {
        public DbSet<Domain.Entities.User> Users => Set<Domain.Entities.User>();
        public DbSet<Domain.Entities.Organization> Organizations => Set<Domain.Entities.Organization>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasQueryFilter(u =>
                !currentTenantService.OrganizationId.HasValue ||
                u.OrganizationId == currentTenantService.OrganizationId);
        }
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {   
                foreach (var entry in ChangeTracker.Entries<IHasOrganization>())
                {
                    if (entry.State == EntityState.Added && currentTenantService.OrganizationId.HasValue)
                    {
                        if (entry.Entity.OrganizationId == 0)
                        {
                            entry.Entity.OrganizationId = currentTenantService.OrganizationId.Value;
                        }
                    }
                }
                return base.SaveChangesAsync(cancellationToken);
            }
    }
}
