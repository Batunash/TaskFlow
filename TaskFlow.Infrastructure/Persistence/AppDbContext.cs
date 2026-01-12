using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks; 
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Persistence
{
    public class AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<TaskItem> TaskItems => Set<TaskItem>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var currentTenantId = currentTenantService.OrganizationId;
            var currentUserId = currentUserService.UserId;
            modelBuilder.Entity<User>().HasQueryFilter(u =>
                 !currentTenantId.HasValue ||
                 u.OrganizationId == currentTenantId);
            modelBuilder.Entity<Project>().HasQueryFilter(p =>
                !currentTenantId.HasValue || p.OrganizationId == currentTenantId
 );

            modelBuilder.Entity<TaskItem>().HasQueryFilter(t =>
                !currentTenantId.HasValue || t.OrganizationId == currentTenantId
            );
            modelBuilder.Entity<ProjectMember>(entity =>
            {
                entity.HasKey(pm => new { pm.ProjectId, pm.UserId });

                entity.HasQueryFilter(pm => !currentTenantId.HasValue || pm.OrganizationId == currentTenantId);

                entity.HasOne(pm => pm.Project)
                      .WithMany(p => p.ProjectMembers)
                      .HasForeignKey(pm => pm.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pm => pm.User)
                      .WithMany(u => u.ProjectMembership)
                      .HasForeignKey(pm => pm.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

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
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}