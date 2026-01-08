using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Enums;
namespace TaskFlow.Domain.Entities
{
    public class Project : IHasOrganization
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public int OrganizationId { get; set; }
        public Organization? Organization { get; private set; }

        private readonly List<TaskItem> _tasks = new();
        public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();
        private Project() { }
        public Project(string name, int organizationId)
        {
            Name = name;
            OrganizationId = organizationId;
        }
        public TaskItem CreateTask(string title)
        {
            var task = new TaskItem(title,Id, OrganizationId);
            _tasks.Add(task);
            return task;
        }

    }
}
