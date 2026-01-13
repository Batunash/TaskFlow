using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Application.DTOs
{
    public class WorkflowDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        public List<WorkflowStateDto> States { get; set; } = [];
        public List<WorkflowTransitionDto> Transitions { get; set; } = [];
    }
}
