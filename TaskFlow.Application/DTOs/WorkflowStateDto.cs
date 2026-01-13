using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.DTOs
{
    public class WorkflowStateDto
    {
        public int? Id { get; set; }  
        public string Name { get; set; } = null!;
        public bool IsInitial { get; set; }
        public bool IsFinal { get; set; }
    }
}
