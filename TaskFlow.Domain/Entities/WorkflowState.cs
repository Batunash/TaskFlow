using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.Entities
{
    public class WorkflowState
    {
        public int Id { get; private set; }
        public int WorkflowId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public bool IsInitial { get; private set; }
        public bool IsFinal { get; private set; }
        private WorkflowState() { }
        public WorkflowState(int workflowId,string name,bool isInitial = false,bool isFinal = false)
        {
            WorkflowId = workflowId;
            Name = name;
            IsInitial = isInitial;
            IsFinal = isFinal;
        }
    }
}

