using System;
using System.Collections.Generic;
using System.Text;
namespace TaskFlow.Domain.Entities
{
    public class TaskItem : IHasProject,IHasOrganization, IAuditableEntity,ISoftDelete
    {
        public int Id { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public int WorkflowStateId { get; private set; }
        public WorkflowState? WorkflowState { get; private set; }
        public int? AssignedUserId { get; private set; }
        public int ProjectId { get; set;}
        public int OrganizationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public Project? Project { get; private set; }
        private TaskItem() { }
        public TaskItem(string title, int projectId, int organizationId,int initialWorkflowStateId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                 throw new ArgumentException("Title cannot be empty", nameof(title));
            }
            Title = title;
            ProjectId = projectId;
            OrganizationId = organizationId;
            WorkflowStateId = initialWorkflowStateId;
        }
        public void ChangeState(int newStateId)
        {
            WorkflowStateId = newStateId;
        }
        public void Delete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
        public void Assign(int userId)
        {
            AssignedUserId = userId;
        }
        public void Update(string title, string description)
        {
            Title = title;
            Description = description;
        }
    }
}
