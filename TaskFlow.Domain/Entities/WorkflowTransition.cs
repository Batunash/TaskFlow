using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.Entities
{
    public class WorkflowTransition
    {
        public int Id { get; private set; }

        public int WorkflowId { get; private set; }

        public int FromStateId { get; private set; }
        public int ToStateId { get; private set; }

        private readonly List<string> _allowedRoles = new();
        public IReadOnlyCollection<string> AllowedRoles => _allowedRoles;
        private WorkflowTransition() { }

        public WorkflowTransition(int workflowId,int fromStateId,int toStateId,IEnumerable<string> allowedRoles)
        {
            WorkflowId = workflowId;
            FromStateId = fromStateId;
            ToStateId = toStateId;
            _allowedRoles.AddRange(allowedRoles);
        }
        public bool IsRoleAllowed(string role)
        {
            return _allowedRoles.Contains(role);
        }
    }
}
