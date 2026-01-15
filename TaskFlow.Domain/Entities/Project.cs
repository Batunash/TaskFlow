using System;
using System.Collections.Generic;
using System.Linq; 
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities
{
    public class Project : IHasOrganization, IAuditableEntity, ISoftDelete
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        public int OrganizationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public Organization? Organization { get; private set; }
        private readonly List<TaskItem> _tasks = new();
        public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();
        public ICollection<ProjectMember> ProjectMembers { get; private set; } = new List<ProjectMember>();

        private Project() { }

        public Project(string name, string description,int organizationId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Project name cannot be empty.", nameof(name));
            }
            Name = name;
            Description = description;
            OrganizationId = organizationId;
        }
        public void Update(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Project name cannot be empty.", nameof(name));
            }
            Name = name;
            Description = description;
        }

        public TaskItem CreateTask(string title, int initialWorkflowStateId)
        {
            var task = new TaskItem(title, Id, OrganizationId,initialWorkflowStateId);
            _tasks.Add(task);
            return task;
        }

        public void AddMember(int userId, Role role)
        {
            if (ProjectMembers.Any(m => m.UserId == userId))
                return;

            ProjectMembers.Add(new ProjectMember(
                Id,
                userId,
                role,
                OrganizationId
            ));
        }

        public bool IsMember(int userId)
        {
            return ProjectMembers.Any(m => m.UserId == userId);
        }

        public bool IsAdmin(int userId)
        {
            return ProjectMembers.Any(m =>
                m.UserId == userId && m.Role == Role.Admin
            );
        }
        public void RemoveMember(int userId)
        {
            var member = ProjectMembers.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return;
            ProjectMembers.Remove(member);
        }

    }
}