using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks; 
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Events;

namespace TaskFlow.Infrastructure.Persistence
{
    public class AppDbContext(
        DbContextOptions<AppDbContext> options,ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<TaskItem> TaskItems => Set<TaskItem>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<Workflow> Workflows => Set<Workflow>();
        public DbSet<WorkflowState> WorkflowStates => Set<WorkflowState>();
        public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var currentTenantId = currentTenantService.OrganizationId;
            var currentUserId = currentUserService.UserId;
            modelBuilder.Entity<TaskItem>().HasQueryFilter(x => !x.IsDeleted);
            modelBuilder.Entity<Project>().HasQueryFilter(p =>
                !currentTenantId.HasValue || p.OrganizationId == currentTenantId
            );
            modelBuilder.Entity<TaskItem>().HasQueryFilter(t =>
                !currentTenantId.HasValue || t.OrganizationId == currentTenantId
            );
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasOne(t => t.WorkflowState)
                      .WithMany()
                      .HasForeignKey(t => t.WorkflowStateId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
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
            modelBuilder.Entity<WorkflowTransition>(entity =>
            {
                entity.Property(e => e.AllowedRoles)
                    .HasConversion(
                        v => string.Join(",", v),
                        v => v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                    )
                    .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyCollection<string>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));
            });
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var currentUserId = currentUserService.UserId?.ToString();
            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = currentUserId;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedAt = DateTime.UtcNow;
                        entry.Entity.LastModifiedBy = currentUserId;
                        break;
                }
            }
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
            foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.DeletedBy = currentUserId;
                }
            }
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}