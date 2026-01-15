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

    public class AppDbContext : DbContext
    {
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ICurrentUserService _currentUserService;
        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenantService currentTenantService, ICurrentUserService currentUserService) : base(options)
        {
            _currentTenantService = currentTenantService;
            _currentUserService = currentUserService;
        }
        internal int? CurrentTenantId => _currentTenantService.OrganizationId;
        internal int? CurrentUserId => _currentUserService.UserId;
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
            modelBuilder.Entity<Project>().HasQueryFilter(p =>
                !p.IsDeleted &&
                (!CurrentTenantId.HasValue || p.OrganizationId == CurrentTenantId)
            );
            modelBuilder.Entity<TaskItem>().HasQueryFilter(t =>
                !t.IsDeleted &&
                (!CurrentTenantId.HasValue || t.OrganizationId == CurrentTenantId)
            );
            modelBuilder.Entity<ProjectMember>().HasQueryFilter(pm =>
                !CurrentTenantId.HasValue || pm.OrganizationId == CurrentTenantId
            );
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasOne(t => t.WorkflowState)
                      .WithMany()
                      .HasForeignKey(t => t.WorkflowStateId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Organization>()
                .HasIndex(o => o.Name)
                .IsUnique();
            modelBuilder.Entity<Project>()
                .HasIndex(p => new { p.OrganizationId, p.Name })
                .IsUnique();
            modelBuilder.Entity<Organization>()
                .Navigation(o => o.Members)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<Workflow>()
                .Navigation(w => w.States)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<Workflow>()
                .Navigation(w => w.Transitions)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<Project>()
                .Navigation(p => p.Tasks)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<ProjectMember>(entity =>
            {
                entity.HasKey(pm => new { pm.ProjectId, pm.UserId });

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
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    )
                    .Metadata.SetValueComparer(
                        new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyCollection<string>>(
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToList()
                        )
                    );
            });

        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,CancellationToken cancellationToken = default)
        {
            var currentUserIdStr = CurrentUserId?.ToString();
            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.CreatedBy = currentUserIdStr;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedAt = DateTime.UtcNow;
                        entry.Entity.LastModifiedBy = currentUserIdStr;
                        break;
                }
            }
            foreach (var entry in ChangeTracker.Entries<IHasOrganization>())
            {
                if (entry.State == EntityState.Added && CurrentTenantId.HasValue)
                {
                    if (entry.Entity.OrganizationId == 0)
                    {
                        entry.Entity.OrganizationId = CurrentTenantId.Value;
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
                    entry.Entity.DeletedBy = currentUserIdStr;
                }
            }
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}