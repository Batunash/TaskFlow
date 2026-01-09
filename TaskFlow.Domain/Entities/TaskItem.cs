using System;
using System.Collections.Generic;
using System.Text;
using TaskFlow.Domain.Enums;
namespace TaskFlow.Domain.Entities
{
    public class TaskItem : IHasProject,IHasOrganization
    {
        public int Id { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public TaskItemStatus Status { get; private set; } = TaskItemStatus.ToDo;
        public int? AssignedUserId { get; private set; }
        public bool IsDeleted { get; private set; }
        public int ProjectId { get; set;}
        public int OrganizationId { get; set; }
        public Project? Project { get; private set; }
        private TaskItem() { }
        internal TaskItem(string title, int projectId, int organizationId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                 throw new ArgumentException("Title cannot be empty", nameof(title));
            }
            Title = title;
            ProjectId = projectId;
            OrganizationId = organizationId;
            Status = TaskItemStatus.ToDo;
        }
        public void Start()
        {
            if (Status != TaskItemStatus.ToDo)
            {
                throw new InvalidOperationException();
            }
            Status = TaskItemStatus.InProgress;
        }
        public void Complete()
        {
            if (Status != TaskItemStatus.InProgress)
            {
                throw new InvalidOperationException();
            }
            Status = TaskItemStatus.Done;
        }
        public void Delete()
        {
            IsDeleted = true;
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
